using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.Helpers;

namespace Testify.Shared.DTOs.TestRunStepAttachments
{
    public class TestRunStepAttachmentResponse
    {
        public int Id { get; set; }
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;

        // Audit fields
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

    }

    public class CreateTestRunStepAttachmentRequest
    {
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class UpdateTestRunStepAttachmentRequest
    {
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}


