using System;
using System.Collections.Generic;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Meetings
{
    public class MeetingResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string HostUserId { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string? HostAvatarUrl { get; set; }
        public MeetingStatus Status { get; set; }
        public int MaxDurationMinutes { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool HasSummary { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MeetingParticipantResponse> Participants { get; set; } = new();
    }
}
