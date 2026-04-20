using System;
using System.Collections.Generic;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class Meeting : AuditEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HostUserId { get; set; } = string.Empty;
        public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
        public int MaxDurationMinutes { get; set; } = 20;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? SummaryContent { get; set; }
        public DateTime? SummaryGeneratedAt { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ApplicationUser Host { get; set; } = null!;
        public virtual ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
        public virtual ICollection<MeetingTranscript> Transcripts { get; set; } = new List<MeetingTranscript>();
    }
}
