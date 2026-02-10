using System.Collections.Generic;
using System.Threading.Tasks;
using Testify.Shared.DTOs.Chat;

namespace Testify.Client.Interfaces
{
    /// <summary>
    /// Represents a file selected for upload.
    /// </summary>
    public class FileUploadItem
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = "application/octet-stream";
        public Stream Content { get; set; } = Stream.Null;
    }

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
        Task<(List<ChatMessageResponse> Messages, int TotalCount)> SearchMessagesAsync(int roomId, string query, int skip = 0, int take = 20);
        Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<ChatMessageResponse> SendMessageWithAttachmentsAsync(int roomId, string? content, int? parentMessageId, IReadOnlyList<FileUploadItem> files);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent);
        
        // Reactions
        Task<bool> AddReactionAsync(AddReactionRequest request);
        Task<bool> RemoveReactionAsync(int messageId, string emoji);
        
        // Read Receipts
        Task<bool> MarkAsReadAsync(int roomId, int messageId);
        Task<int> GetUnreadCountAsync(int roomId);

        // Pinned Messages
        Task<ChatPinnedMessageResponse> PinMessageAsync(int roomId, int messageId, string? note = null);
        Task<bool> UnpinMessageAsync(int roomId, int messageId);
        Task<List<ChatPinnedMessageResponse>> GetPinnedMessagesAsync(int roomId);

        // Room Settings
        Task<ChatRoomResponse?> UpdateRoomAsync(int roomId, UpdateRoomRequest request);
        Task<bool> LeaveRoomAsync(int roomId);
        Task<List<ChatParticipantResponse>> AddMembersAsync(int roomId, List<string> userIds);
        Task<bool> RemoveMemberAsync(int roomId, string memberId);
        Task<bool> ToggleMuteAsync(int roomId);
    }
}
