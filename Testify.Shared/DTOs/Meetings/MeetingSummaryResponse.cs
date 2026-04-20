using System;

namespace Testify.Shared.DTOs.Meetings
{
    public class MeetingSummaryResponse
    {
        public int MeetingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime? GeneratedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int ParticipantCount { get; set; }
        public int AttendedCount { get; set; }
    }
}
