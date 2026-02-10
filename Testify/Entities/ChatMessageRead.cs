using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ChatMessageRead
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ReadAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual ChatMessage Message { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
