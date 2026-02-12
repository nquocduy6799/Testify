using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Hubs
{
    [Authorize]
    public class CallHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;

        // Track active calls per user: UserId -> CallSessionId
        // Prevents a user from being in multiple calls simultaneously
        private static readonly ConcurrentDictionary<string, int> ActiveUserCalls = new();

        public CallHub(ApplicationDbContext context, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _chatHub = chatHub;
        }

        private string GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User not authenticated");
        }

        private string GetCurrentUserName()
        {
            return Context.User?.Identity?.Name ?? "Unknown";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            // Add user to their personal group for targeted signaling
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();

            // If user was in an active call, end it
            if (ActiveUserCalls.TryRemove(userId, out var callSessionId))
            {
                await EndCallInternal(callSessionId, userId, CallStatus.Ended);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Caller initiates a call. Creates a CallSession and sends offer to callee.
        /// </summary>
        public async Task SendOffer(CallOfferRequest request)
        {
            var callerId = GetCurrentUserId();

            // Check caller is not already in a call
            if (ActiveUserCalls.ContainsKey(callerId))
            {
                await Clients.Caller.SendAsync("CallError", "You are already in a call.");
                return;
            }

            // Validate room exists and is Private (1-1 only)
            var room = await _context.ChatRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.RoomType == ChatRoomType.Private && !r.IsDeleted);

            if (room == null)
            {
                await Clients.Caller.SendAsync("CallError", "Room not found or not a private chat.");
                return;
            }

            // Validate caller is a participant
            if (!room.Participants.Any(p => p.UserId == callerId))
            {
                await Clients.Caller.SendAsync("CallError", "You are not a participant of this room.");
                return;
            }

            // Find the other participant (callee)
            var calleeParticipant = room.Participants.FirstOrDefault(p => p.UserId != callerId);
            if (calleeParticipant == null)
            {
                await Clients.Caller.SendAsync("CallError", "No other participant found.");
                return;
            }

            var calleeId = calleeParticipant.UserId;

            // Check callee is not already in a call
            if (ActiveUserCalls.ContainsKey(calleeId))
            {
                await Clients.Caller.SendAsync("CallError", "The other user is currently in another call.");
                return;
            }

            // Get caller info for incoming call UI
            var caller = await _context.Users.FindAsync(callerId);

            // Create call session in DB
            var callSession = new CallSession
            {
                RoomId = request.RoomId,
                CallerUserId = callerId,
                CalleeUserId = calleeId,
                CallType = request.CallType,
                Status = CallStatus.Ringing,
                StartedAt = DateTimeHelper.GetVietnamTime()
            };

            _context.CallSessions.Add(callSession);
            await _context.SaveChangesAsync();

            // Track both users as in call
            ActiveUserCalls[callerId] = callSession.Id;
            ActiveUserCalls[calleeId] = callSession.Id;

            // Send incoming call notification to callee
            var incomingCall = new IncomingCallResponse
            {
                CallSessionId = callSession.Id,
                RoomId = request.RoomId,
                CallType = request.CallType,
                CallerUserId = callerId,
                CallerName = caller?.FullName ?? caller?.UserName ?? "Unknown",
                CallerAvatarUrl = caller?.AvatarUrl,
                SdpOffer = request.SdpOffer
            };

            await Clients.Group($"user_{calleeId}").SendAsync("IncomingCall", incomingCall);

            // Confirm to caller that call is ringing
            await Clients.Caller.SendAsync("CallRinging", callSession.Id);
        }

        /// <summary>
        /// Callee accepts the call. Sends answer back to caller.
        /// </summary>
        public async Task SendAnswer(CallAnswerRequest request)
        {
            var calleeId = GetCurrentUserId();

            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == request.CallSessionId && c.CalleeUserId == calleeId && c.Status == CallStatus.Ringing);

            if (callSession == null)
            {
                await Clients.Caller.SendAsync("CallError", "Call session not found or already ended.");
                return;
            }

            // Update call status
            callSession.Status = CallStatus.Active;
            callSession.AnsweredAt = DateTimeHelper.GetVietnamTime();
            await _context.SaveChangesAsync();

            // Send answer to caller
            var response = new CallAnsweredResponse
            {
                CallSessionId = callSession.Id,
                SdpAnswer = request.SdpAnswer
            };

            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallAnswered", response);
        }

        /// <summary>
        /// Exchange ICE candidates between peers.
        /// </summary>
        public async Task SendIceCandidate(IceCandidateRequest request)
        {
            var userId = GetCurrentUserId();

            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == request.CallSessionId
                    && (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Active));

            if (callSession == null) return;

            // Send ICE candidate to the other peer
            var targetUserId = callSession.CallerUserId == userId
                ? callSession.CalleeUserId
                : callSession.CallerUserId;

            await Clients.Group($"user_{targetUserId}").SendAsync("IceCandidateReceived", request);
        }

        /// <summary>
        /// Either party ends the active call.
        /// </summary>
        public async Task EndCall(int callSessionId)
        {
            var userId = GetCurrentUserId();
            await EndCallInternal(callSessionId, userId, CallStatus.Ended);
        }

        /// <summary>
        /// Callee rejects the incoming call.
        /// </summary>
        public async Task RejectCall(int callSessionId)
        {
            var userId = GetCurrentUserId();

            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.CalleeUserId == userId && c.Status == CallStatus.Ringing);

            if (callSession == null) return;

            callSession.Status = CallStatus.Rejected;
            callSession.EndedAt = DateTimeHelper.GetVietnamTime();
            await _context.SaveChangesAsync();

            // Remove from active calls
            ActiveUserCalls.TryRemove(callSession.CallerUserId, out _);
            ActiveUserCalls.TryRemove(callSession.CalleeUserId, out _);

            // Notify caller
            var response = new CallEndedResponse
            {
                CallSessionId = callSession.Id,
                Reason = CallStatus.Rejected
            };

            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallEnded", response);
            await Clients.Group($"user_{callSession.CalleeUserId}").SendAsync("CallEnded", response);

            // Save call history message in chat
            await CreateCallMessageAsync(callSession, CallStatus.Rejected, null);
        }

        /// <summary>
        /// Caller cancels the outgoing call before it's answered.
        /// </summary>
        public async Task CancelCall(int callSessionId)
        {
            var userId = GetCurrentUserId();

            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.CallerUserId == userId && c.Status == CallStatus.Ringing);

            if (callSession == null) return;

            callSession.Status = CallStatus.Cancelled;
            callSession.EndedAt = DateTimeHelper.GetVietnamTime();
            await _context.SaveChangesAsync();

            // Remove from active calls
            ActiveUserCalls.TryRemove(callSession.CallerUserId, out _);
            ActiveUserCalls.TryRemove(callSession.CalleeUserId, out _);

            // Notify callee
            var response = new CallEndedResponse
            {
                CallSessionId = callSession.Id,
                Reason = CallStatus.Cancelled
            };

            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallEnded", response);
            await Clients.Group($"user_{callSession.CalleeUserId}").SendAsync("CallEnded", response);

            // Save call history message in chat
            await CreateCallMessageAsync(callSession, CallStatus.Cancelled, null);
        }

        /// <summary>
        /// Toggle media state and notify the other peer.
        /// </summary>
        public async Task ToggleMedia(int callSessionId, bool isAudioEnabled, bool isVideoEnabled)
        {
            var userId = GetCurrentUserId();

            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId
                    && (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && c.Status == CallStatus.Active);

            if (callSession == null) return;

            var targetUserId = callSession.CallerUserId == userId
                ? callSession.CalleeUserId
                : callSession.CallerUserId;

            await Clients.Group($"user_{targetUserId}")
                .SendAsync("MediaToggled", userId, isAudioEnabled, isVideoEnabled);
        }

        #region Private Methods

        private async Task EndCallInternal(int callSessionId, string userId, CallStatus reason)
        {
            var callSession = await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId
                    && (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Active));

            if (callSession == null) return;

            var wasRinging = callSession.Status == CallStatus.Ringing;
            callSession.Status = wasRinging ? CallStatus.Missed : reason;
            callSession.EndedAt = DateTimeHelper.GetVietnamTime();
            await _context.SaveChangesAsync();

            // Remove from active calls
            ActiveUserCalls.TryRemove(callSession.CallerUserId, out _);
            ActiveUserCalls.TryRemove(callSession.CalleeUserId, out _);

            int? durationSeconds = null;
            if (callSession.AnsweredAt.HasValue && callSession.EndedAt.HasValue)
            {
                durationSeconds = (int)(callSession.EndedAt.Value - callSession.AnsweredAt.Value).TotalSeconds;
            }

            var response = new CallEndedResponse
            {
                CallSessionId = callSession.Id,
                Reason = callSession.Status,
                DurationSeconds = durationSeconds
            };

            // Notify both users
            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallEnded", response);
            await Clients.Group($"user_{callSession.CalleeUserId}").SendAsync("CallEnded", response);

            // Save call history message in chat
            await CreateCallMessageAsync(callSession, callSession.Status, durationSeconds);
        }

        /// <summary>
        /// Creates a chat message of type Call to record call history in the chat room.
        /// Broadcasts the message to both participants via ChatHub.
        /// </summary>
        private async Task CreateCallMessageAsync(CallSession callSession, CallStatus status, int? durationSeconds)
        {
            var now = DateTimeHelper.GetVietnamTime();

            var metadata = JsonSerializer.Serialize(new
            {
                callSessionId = callSession.Id,
                callType = callSession.CallType.ToString(),
                callStatus = status.ToString(),
                durationSeconds = durationSeconds,
                callerUserId = callSession.CallerUserId,
                calleeUserId = callSession.CalleeUserId
            });

            // Determine display content based on status
            var callTypeLabel = callSession.CallType == CallType.Video ? "Video" : "Voice";
            var content = status switch
            {
                CallStatus.Ended => durationSeconds.HasValue
                    ? $"{callTypeLabel} call - {FormatDuration(durationSeconds.Value)}"
                    : $"{callTypeLabel} call ended",
                CallStatus.Missed => $"Missed {callTypeLabel.ToLower()} call",
                CallStatus.Rejected => $"{callTypeLabel} call declined",
                CallStatus.Cancelled => $"{callTypeLabel} call cancelled",
                _ => $"{callTypeLabel} call"
            };

            var callMessage = new ChatMessage
            {
                RoomId = callSession.RoomId,
                UserId = callSession.CallerUserId,
                MessageType = MessageType.Call,
                Content = content,
                Metadata = metadata,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = callSession.CallerUserId,
                UpdatedBy = callSession.CallerUserId
            };

            _context.ChatMessages.Add(callMessage);

            // Update room's last activity
            var room = await _context.ChatRooms.FindAsync(callSession.RoomId);
            if (room != null)
            {
                room.LastActivityAt = now;
            }

            await _context.SaveChangesAsync();

            // Reload with User navigation for the response
            var savedMessage = await _context.ChatMessages
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == callMessage.Id);

            if (savedMessage == null) return;

            var messageResponse = new ChatMessageResponse
            {
                Id = savedMessage.Id,
                RoomId = savedMessage.RoomId,
                UserId = savedMessage.UserId,
                UserName = savedMessage.User?.UserName ?? "",
                UserAvatarUrl = savedMessage.User?.AvatarUrl,
                MessageType = MessageType.Call,
                Content = savedMessage.Content,
                IsDeleted = false,
                CreatedAt = savedMessage.CreatedAt,
                Metadata = savedMessage.Metadata
            };

            // Broadcast to both participants via ChatHub
            await _chatHub.Clients.User(callSession.CallerUserId).SendAsync("ReceiveMessage", messageResponse);
            await _chatHub.Clients.User(callSession.CalleeUserId).SendAsync("ReceiveMessage", messageResponse);
        }

        private static string FormatDuration(int totalSeconds)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            return $"{minutes:D2}:{seconds:D2}";
        }

        #endregion
    }
}
