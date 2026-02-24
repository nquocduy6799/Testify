using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.TestSuites
{
    // ── TestSuite (project-level) ──────────────────────────────────────

    public class TestSuiteResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? FolderId { get; set; }
        public int? SourceTemplateId { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public List<TestCaseResponse> TestCases { get; set; } = new();
    }

    public class CreateTestSuiteRequest
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? FolderId { get; set; }

        /// <summary>
        /// If set, the suite (with all its cases &amp; steps) will be cloned from this template.
        /// </summary>
        public int? SourceTemplateId { get; set; }
    }

    public class UpdateTestSuiteRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? FolderId { get; set; }
    }

    // ── TestCase (project-level) ───────────────────────────────────────

    public class TestCaseResponse
    {
        public int Id { get; set; }
        public int SuiteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Preconditions { get; set; }
        public string? Postconditions { get; set; }
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;
        public DateTime? LastRun { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public List<TestStepResponse> TestSteps { get; set; } = new();
    }

    public class CreateTestCaseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Preconditions { get; set; }

        [StringLength(2000)]
        public string? Postconditions { get; set; }

        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        public List<CreateTestStepRequest> TestSteps { get; set; } = new();
    }

    public class UpdateTestCaseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Preconditions { get; set; }

        [StringLength(2000)]
        public string? Postconditions { get; set; }

        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        public List<CreateTestStepRequest> TestSteps { get; set; } = new();
    }

    // ── TestStep (project-level) ───────────────────────────────────────

    public class TestStepResponse
    {
        public int Id { get; set; }
        public int TestCaseId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;
    }

    public class CreateTestStepRequest
    {
        [Range(1, int.MaxValue)]
        public int StepNumber { get; set; }

        [Required]
        [StringLength(1000)]
        public string Action { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? TestData { get; set; }

        [Required]
        [StringLength(1000)]
        public string ExpectedResult { get; set; } = string.Empty;
    }
}
