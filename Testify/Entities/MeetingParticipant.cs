using System;
using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class MeetingParticipant
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool HasAttended { get; set; } = false;

        // Navigation properties
        public virtual Meeting Meeting { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
