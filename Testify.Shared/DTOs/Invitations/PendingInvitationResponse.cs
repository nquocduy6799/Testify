using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Invitations
{
    public class PendingInvitationResponse
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? InviteeName { get; set; }
        public string? SenderName { get; set; }
        public ProjectRole Role { get; set; }
        public InvitationStatus Status { get; set; }
        public DateTime SentAt { get; set; }
    }
}
