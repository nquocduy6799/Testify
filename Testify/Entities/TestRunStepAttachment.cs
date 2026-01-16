using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TestRunStepAttachment
    {
        public int AttachmentId { get; set; }
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long? FileSizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual TestRunStep RunStep { get; set; } = null!;
    }
}
