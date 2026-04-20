using System;

namespace Testify.Shared.DTOs.Meetings
{
    public class MeetingParticipantResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool HasAttended { get; set; }
        public bool IsOnline { get; set; }
    }
}
