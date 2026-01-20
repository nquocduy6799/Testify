using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class Notification : AuditEntity
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Link { get; set; }
        public NotificationType Type { get; set; } = NotificationType.NewMessage;
        public bool IsRead { get; set; } = false;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
