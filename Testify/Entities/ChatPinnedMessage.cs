using System;
using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ChatPinnedMessage
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int MessageId { get; set; }
        public string PinnedByUserId { get; set; } = string.Empty;
        public DateTime PinnedAt { get; set; } = DateTimeHelper.GetVietnamTime();
        public string? Note { get; set; }

        // Navigation properties
        public virtual ChatRoom Room { get; set; } = null!;
        public virtual ChatMessage Message { get; set; } = null!;
        public virtual ApplicationUser PinnedBy { get; set; } = null!;
    }
}
