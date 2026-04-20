namespace Testify.Shared.DTOs.Meetings
{
    public class MeetingChatMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
