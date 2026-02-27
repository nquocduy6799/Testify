using System;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class CallSession
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string CallerUserId { get; set; } = string.Empty;
        public string CalleeUserId { get; set; } = string.Empty;
        public CallType CallType { get; set; }
        public CallStatus Status { get; set; } = CallStatus.Ringing;
        public DateTime StartedAt { get; set; } = DateTimeHelper.GetVietnamTime();
        public DateTime? AnsweredAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Navigation properties
        public virtual ChatRoom Room { get; set; } = null!;
        public virtual ApplicationUser Caller { get; set; } = null!;
        public virtual ApplicationUser Callee { get; set; } = null!;
    }
}
