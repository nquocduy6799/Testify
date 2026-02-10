using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ChatNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public int? MessageId { get; set; }
        public NotificationType NotificationType { get; set; } = NotificationType.NewMessage;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ChatRoom Room { get; set; } = null!;
        public virtual ChatMessage? Message { get; set; }
    }
}
