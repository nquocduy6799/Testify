using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Testify.Interfaces;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;

namespace Testify.Hubs
{
    [Authorize]
    public class CallHub : Hub
    {
        private readonly ICallSessionRepository _callRepo;

        // Track active calls per user: UserId -> CallSessionId
        // Prevents a user from being in multiple calls simultaneously
        // NOTE: Same pattern as ChatHub.RoomConnections — for multi-instance, replace with Redis backplane
        private static readonly ConcurrentDictionary<string, int> ActiveUserCalls = new();

        public CallHub(ICallSessionRepository callRepo)
        {
            _callRepo = callRepo;
        }

        private string GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User not authenticated");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Sync in-memory state from DB on reconnect
            if (!ActiveUserCalls.ContainsKey(userId))
            {
                var activeSession = await _callRepo.GetActiveCallSessionForUserAsync(userId);
                if (activeSession != null)
                {
                    ActiveUserCalls.TryAdd(userId, activeSession.Id);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

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

            // Check caller is not already in a call (in-memory + DB fallback)
            if (ActiveUserCalls.ContainsKey(callerId) || await _callRepo.HasActiveCallAsync(callerId))
            {
                await Clients.Caller.SendAsync("CallError", "You are already in a call.");
                return;
            }

            // Validate room exists and is Private (1-1 only)
            var room = await _callRepo.GetPrivateRoomWithParticipantsAsync(request.RoomId);
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

            // Check callee is not already in a call (in-memory + DB fallback)
            if (ActiveUserCalls.ContainsKey(calleeId) || await _callRepo.HasActiveCallAsync(calleeId))
            {
                await Clients.Caller.SendAsync("CallError", "The other user is currently in another call.");
                return;
            }

            // Get caller info for incoming call UI
            var caller = await _callRepo.GetUserAsync(callerId);

            // Create call session in DB
            var callSession = await _callRepo.CreateCallSessionAsync(request.RoomId, callerId, calleeId, request.CallType);

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

            var callSession = await _callRepo.GetCallSessionForCalleeAsync(request.CallSessionId, calleeId, CallStatus.Ringing);
            if (callSession == null)
            {
                await Clients.Caller.SendAsync("CallError", "Call session not found or already ended.");
                return;
            }

            // Update call status to Active
            await _callRepo.UpdateCallStatusAsync(callSession, CallStatus.Active);

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

            var callSession = await _callRepo.GetCallSessionAsync(request.CallSessionId, userId, CallStatus.Ringing, CallStatus.Active);
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

            var callSession = await _callRepo.GetCallSessionForCalleeAsync(callSessionId, userId, CallStatus.Ringing);
            if (callSession == null) return;

            await _callRepo.UpdateCallStatusAsync(callSession, CallStatus.Rejected);

            // Remove from active calls
            ActiveUserCalls.TryRemove(callSession.CallerUserId, out _);
            ActiveUserCalls.TryRemove(callSession.CalleeUserId, out _);

            // Notify both users
            var response = new CallEndedResponse
            {
                CallSessionId = callSession.Id,
                Reason = CallStatus.Rejected
            };

            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallEnded", response);
            await Clients.Group($"user_{callSession.CalleeUserId}").SendAsync("CallEnded", response);

            // Save call history message in chat
            await _callRepo.CreateCallMessageAsync(callSession, CallStatus.Rejected, null);
        }

        /// <summary>
        /// Caller cancels the outgoing call before it's answered.
        /// </summary>
        public async Task CancelCall(int callSessionId)
        {
            var userId = GetCurrentUserId();

            var callSession = await _callRepo.GetCallSessionForCallerAsync(callSessionId, userId, CallStatus.Ringing);
            if (callSession == null) return;

            await _callRepo.UpdateCallStatusAsync(callSession, CallStatus.Cancelled);

            // Remove from active calls
            ActiveUserCalls.TryRemove(callSession.CallerUserId, out _);
            ActiveUserCalls.TryRemove(callSession.CalleeUserId, out _);

            // Notify both users
            var response = new CallEndedResponse
            {
                CallSessionId = callSession.Id,
                Reason = CallStatus.Cancelled
            };

            await Clients.Group($"user_{callSession.CallerUserId}").SendAsync("CallEnded", response);
            await Clients.Group($"user_{callSession.CalleeUserId}").SendAsync("CallEnded", response);

            // Save call history message in chat
            await _callRepo.CreateCallMessageAsync(callSession, CallStatus.Cancelled, null);
        }

        /// <summary>
        /// Toggle media state and notify the other peer.
        /// </summary>
        public async Task ToggleMedia(int callSessionId, bool isAudioEnabled, bool isVideoEnabled)
        {
            var userId = GetCurrentUserId();

            var callSession = await _callRepo.GetCallSessionAsync(callSessionId, userId, CallStatus.Active);
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
            var callSession = await _callRepo.GetCallSessionAsync(callSessionId, userId, CallStatus.Ringing, CallStatus.Active);
            if (callSession == null) return;

            var wasRinging = callSession.Status == CallStatus.Ringing;
            var finalStatus = wasRinging ? CallStatus.Missed : reason;

            await _callRepo.UpdateCallStatusAsync(callSession, finalStatus);

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
            await _callRepo.CreateCallMessageAsync(callSession, callSession.Status, durationSeconds);
        }

        #endregion

        /// <summary>
        /// Client can call this after reconnect to check if they have an active call.
        /// </summary>
        public async Task<ActiveCallInfoResponse?> GetActiveCall()
        {
            var userId = GetCurrentUserId();

            if (!ActiveUserCalls.TryGetValue(userId, out var callSessionId))
            {
                // Also check DB as fallback
                var dbSession = await _callRepo.GetActiveCallSessionForUserAsync(userId);
                if (dbSession == null) return null;
                callSessionId = dbSession.Id;
                ActiveUserCalls.TryAdd(userId, callSessionId);
            }

            var session = await _callRepo.GetCallSessionByIdAsync(callSessionId);
            if (session == null || (session.Status != CallStatus.Ringing && session.Status != CallStatus.Active))
            {
                ActiveUserCalls.TryRemove(userId, out _);
                return null;
            }

            var peerId = session.CallerUserId == userId ? session.CalleeUserId : session.CallerUserId;
            var peer = await _callRepo.GetUserAsync(peerId);

            return new ActiveCallInfoResponse
            {
                CallSessionId = session.Id,
                CallType = session.CallType,
                Status = session.Status,
                PeerName = peer?.FullName ?? peer?.UserName ?? "Unknown",
                PeerAvatarUrl = peer?.AvatarUrl,
                AnsweredAt = session.AnsweredAt
            };
        }
    }
}
