using System.Collections.Generic;
using System.Threading.Tasks;
using Testify.Entities;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface IChatRepository
    {
        // Rooms
        Task<List<ChatRoomResponse>> GetUserChatRoomsAsync(string userId);
        Task<ChatRoomResponse?> GetChatRoomByIdAsync(int roomId);
        Task<bool> IsUserInRoomAsync(int roomId, string userId);
        Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request, string currentUserId);
        Task<ChatRoomResponse?> GetOrCreateDirectRoomAsync(string userId1, string userId2);
        Task<ChatRoomResponse?> UpdateRoomAsync(int roomId, UpdateRoomRequest request, string currentUserId);
        Task<bool> LeaveRoomAsync(int roomId, string userId);
        Task<bool> ToggleMuteAsync(int roomId, string userId);
        Task<ChatParticipantRole?> GetUserRoleInRoomAsync(int roomId, string userId);
        
        // Messages
        Task<List<ChatMessageResponse>> GetRoomMessagesAsync(int roomId, int skip = 0, int take = 50);
        Task<(List<ChatMessageResponse> Messages, int TotalCount)> SearchMessagesAsync(int roomId, string query, int skip = 0, int take = 20);
        Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, string currentUserId);
        Task<ChatMessageResponse> SendMessageWithAttachmentsAsync(SendMessageRequest request, string currentUserId, List<ChatMessageAttachment> attachments);
        Task<int?> GetMessageRoomIdAsync(int messageId);
        Task<bool> DeleteMessageAsync(int messageId, string currentUserId);
        Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent, string currentUserId);
        Task<ChatMessageAttachment?> GetAttachmentByIdAsync(int attachmentId);
        
        // Reactions
        Task<bool> AddReactionAsync(AddReactionRequest request, string currentUserId);
        Task<bool> RemoveReactionAsync(int messageId, string emoji, string currentUserId);
        
        // Read Receipts
        Task<bool> MarkAsReadAsync(int roomId, int messageId, string currentUserId);
        Task<int> GetUnreadCountAsync(int roomId, string currentUserId);
        
        // Participants
        Task<List<ChatParticipantResponse>> GetRoomParticipantsAsync(int roomId);
        Task<bool> AddParticipantAsync(int roomId, string userId, string addedBy);
        Task<bool> RemoveParticipantAsync(int roomId, string userId, string removedBy);

        // Pinned Messages
        Task<ChatPinnedMessageResponse> PinMessageAsync(int roomId, int messageId, string userId, string? note);
        Task<bool> UnpinMessageAsync(int roomId, int messageId, string userId);
        Task<List<ChatPinnedMessageResponse>> GetPinnedMessagesAsync(int roomId);
        Task<bool> IsMessagePinnedAsync(int roomId, int messageId);
    }
}
