using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ChatRoomParticipant
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTimeHelper.GetVietnamTime();
        public ChatParticipantRole Role { get; set; } = ChatParticipantRole.Member;
        public bool IsMuted { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public int? LastReadMessageId { get; set; }

        // Navigation properties
        public virtual ChatRoom Room { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
