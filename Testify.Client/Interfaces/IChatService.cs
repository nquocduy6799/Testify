using System.Collections.Generic;
using System.Threading.Tasks;
using Testify.Shared.DTOs.Chat;

namespace Testify.Client.Interfaces
{
    public interface IChatService
    {
        // Rooms
        Task<List<ChatRoomResponse>> GetChatRoomsAsync();
        Task<ChatRoomResponse?> GetChatRoomByIdAsync(int roomId);
        Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request);
        Task<ChatRoomResponse?> GetOrCreateDirectRoomAsync(string otherUserId);
        Task<List<ChatUserItemResponse>> GetUsersForChatAsync(string? search = null);
        Task<string?> GetCurrentUserIdAsync();
        
        // Messages
        Task<List<ChatMessageResponse>> GetRoomMessagesAsync(int roomId, int skip = 0, int take = 50);
        Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent);
        
        // Reactions
        Task<bool> AddReactionAsync(AddReactionRequest request);
        Task<bool> RemoveReactionAsync(int messageId, string emoji);
        
        // Read Receipts
        Task<bool> MarkAsReadAsync(int roomId, int messageId);
        Task<int> GetUnreadCountAsync(int roomId);
    }
}
