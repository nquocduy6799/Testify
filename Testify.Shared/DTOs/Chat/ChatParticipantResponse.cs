using System;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatParticipantResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime JoinedAt { get; set; }
        public ChatParticipantRole Role { get; set; }
        public bool IsMuted { get; set; }
        public bool IsPinned { get; set; }
        public int? LastReadMessageId { get; set; }
        
        // Real-time status (not stored in DB, fetched from SignalR)
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Offline"; // Online, Offline, Meeting, Busy
    }
}
