using System;
using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class MeetingTranscript
    {
        public long Id { get; set; }
        public int MeetingId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual Meeting Meeting { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
