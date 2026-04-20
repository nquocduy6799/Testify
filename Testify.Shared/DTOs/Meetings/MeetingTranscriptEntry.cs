using System;

namespace Testify.Shared.DTOs.Meetings
{
    public class MeetingTranscriptEntry
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
