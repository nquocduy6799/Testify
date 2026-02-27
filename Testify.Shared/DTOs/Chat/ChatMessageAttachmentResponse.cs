using System;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatMessageAttachmentResponse
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
