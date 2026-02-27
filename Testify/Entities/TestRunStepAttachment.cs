using System;
using System.Collections.Generic;
using System.Text;
using Testify.Entities;

namespace Testify.Entities
{
    public class TestRunStepAttachment : AuditEntity
    {
        public int Id { get; set; }
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;

        // Navigation properties
        public virtual TestRunStep RunStep { get; set; } = null!;
    }
}
