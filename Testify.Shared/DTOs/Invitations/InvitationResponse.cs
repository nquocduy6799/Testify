namespace Testify.Shared.DTOs.Invitations
{
    public class InvitationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? NotificationId { get; set; }
    }
}
