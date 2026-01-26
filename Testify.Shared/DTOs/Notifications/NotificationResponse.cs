using System;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Notifications
{
    public class NotificationResponse
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Link { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Invitation-specific metadata
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? SenderName { get; set; }
        public InvitationStatus? InvitationStatus { get; set; }
    }
}
