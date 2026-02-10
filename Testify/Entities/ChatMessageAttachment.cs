using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ChatMessageAttachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual ChatMessage Message { get; set; } = null!;
    }
}
