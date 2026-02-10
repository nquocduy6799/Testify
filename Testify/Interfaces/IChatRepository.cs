using System.Collections.Generic;
using System.Threading.Tasks;
using Testify.Shared.DTOs.Chat;

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
        
        // Messages
        Task<List<ChatMessageResponse>> GetRoomMessagesAsync(int roomId, int skip = 0, int take = 50);
        Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, string currentUserId);
        Task<int?> GetMessageRoomIdAsync(int messageId);
        Task<bool> DeleteMessageAsync(int messageId, string currentUserId);
        Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent, string currentUserId);
        
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
    }
}
