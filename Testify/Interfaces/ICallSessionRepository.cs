using Testify.Data;
using Testify.Entities;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ICallSessionRepository
    {
        /// <summary>
        /// Get a private chat room with participants loaded.
        /// </summary>
        Task<ChatRoom?> GetPrivateRoomWithParticipantsAsync(int roomId);

        /// <summary>
        /// Get user info by ID.
        /// </summary>
        Task<ApplicationUser?> GetUserAsync(string userId);

        /// <summary>
        /// Check if a user currently has an active or ringing call session.
        /// </summary>
        Task<bool> HasActiveCallAsync(string userId);

        /// <summary>
        /// Create a new call session in the database.
        /// </summary>
        Task<CallSession> CreateCallSessionAsync(int roomId, string callerId, string calleeId, CallType callType);

        /// <summary>
        /// Get a call session by ID, validating that the user is a participant and the status matches.
        /// </summary>
        Task<CallSession?> GetCallSessionAsync(int callSessionId, string userId, params CallStatus[] validStatuses);

        /// <summary>
        /// Get a call session for the callee only (for reject/answer operations).
        /// </summary>
        Task<CallSession?> GetCallSessionForCalleeAsync(int callSessionId, string calleeId, CallStatus requiredStatus);

        /// <summary>
        /// Get a call session for the caller only (for cancel operations).
        /// </summary>
        Task<CallSession?> GetCallSessionForCallerAsync(int callSessionId, string callerId, CallStatus requiredStatus);

        /// <summary>
        /// Update call session status and set AnsweredAt/EndedAt as appropriate.
        /// </summary>
        Task UpdateCallStatusAsync(CallSession session, CallStatus newStatus);

        /// <summary>
        /// Create a ChatMessage of type Call to record call history in the chat room.
        /// Returns the message response for broadcasting.
        /// </summary>
        Task<ChatMessageResponse?> CreateCallMessageAsync(CallSession session, CallStatus status, int? durationSeconds);
    }
}
