using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Testify.Data;
using Testify.Entities;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class CallSessionRepository : ICallSessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;

        public CallSessionRepository(ApplicationDbContext context, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _chatHub = chatHub;
        }

        public async Task<ChatRoom?> GetPrivateRoomWithParticipantsAsync(int roomId)
        {
            return await _context.ChatRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == roomId && r.RoomType == ChatRoomType.Private && !r.IsDeleted);
        }

        public async Task<ApplicationUser?> GetUserAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> HasActiveCallAsync(string userId)
        {
            return await _context.CallSessions
                .AnyAsync(c => (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Active));
        }

        public async Task<CallSession> CreateCallSessionAsync(int roomId, string callerId, string calleeId, CallType callType)
        {
            var callSession = new CallSession
            {
                RoomId = roomId,
                CallerUserId = callerId,
                CalleeUserId = calleeId,
                CallType = callType,
                Status = CallStatus.Ringing,
                StartedAt = DateTimeHelper.GetVietnamTime()
            };

            _context.CallSessions.Add(callSession);
            await _context.SaveChangesAsync();

            return callSession;
        }

        public async Task<CallSession?> GetCallSessionAsync(int callSessionId, string userId, params CallStatus[] validStatuses)
        {
            return await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId
                    && (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && validStatuses.Contains(c.Status));
        }

        public async Task<CallSession?> GetCallSessionForCalleeAsync(int callSessionId, string calleeId, CallStatus requiredStatus)
        {
            return await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.CalleeUserId == calleeId && c.Status == requiredStatus);
        }

        public async Task<CallSession?> GetCallSessionForCallerAsync(int callSessionId, string callerId, CallStatus requiredStatus)
        {
            return await _context.CallSessions
                .FirstOrDefaultAsync(c => c.Id == callSessionId && c.CallerUserId == callerId && c.Status == requiredStatus);
        }

        public async Task UpdateCallStatusAsync(CallSession session, CallStatus newStatus)
        {
            session.Status = newStatus;

            if (newStatus == CallStatus.Active)
                session.AnsweredAt = DateTimeHelper.GetVietnamTime();

            if (newStatus == CallStatus.Ended || newStatus == CallStatus.Missed
                || newStatus == CallStatus.Rejected || newStatus == CallStatus.Cancelled)
                session.EndedAt = DateTimeHelper.GetVietnamTime();

            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessageResponse?> CreateCallMessageAsync(CallSession callSession, CallStatus status, int? durationSeconds)
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

            var callTypeLabel = callSession.CallType == CallType.Video ? "Video" : "Voice";
            var content = status switch
            {
                CallStatus.Ended => durationSeconds.HasValue
                    ? $"{callTypeLabel} call - {StringHelper.FormatDuration(durationSeconds.Value)}"
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

            if (savedMessage == null) return null;

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

            return messageResponse;
        }

        public async Task<CallSession?> GetActiveCallSessionForUserAsync(string userId)
        {
            return await _context.CallSessions
                .FirstOrDefaultAsync(c => (c.CallerUserId == userId || c.CalleeUserId == userId)
                    && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Active));
        }

        public async Task<CallSession?> GetCallSessionByIdAsync(int callSessionId)
        {
            return await _context.CallSessions.FindAsync(callSessionId);
        }
    }
}
