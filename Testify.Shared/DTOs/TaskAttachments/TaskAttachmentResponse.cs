using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.DTOs.TaskAttachments
{
    public class TaskAttachmentResponse
    {
        public int Id { get; set; }
        public int KanbanTaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
