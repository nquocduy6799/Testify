using System;
using System.Collections.Generic;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatRoomResponse
    {
        public int Id { get; set; }
        public string? RoomName { get; set; }
        public ChatRoomType RoomType { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public DateTime LastActivityAt { get; set; }
        public bool IsArchived { get; set; }
        
        // Last message info
        public string? LastMessage { get; set; }
        public string? LastMessageSender { get; set; }
        public DateTime? LastMessageTime { get; set; }
        
        // Unread count for current user
        public int UnreadCount { get; set; }
        
        // Participants
        public List<ChatParticipantResponse> Participants { get; set; } = new();
        
        // For direct messages (1-on-1)
        public ChatParticipantResponse? OtherParticipant { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
