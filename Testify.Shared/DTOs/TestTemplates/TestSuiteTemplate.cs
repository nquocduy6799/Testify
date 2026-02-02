using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.TestTemplates
{
    public class TestSuiteTemplateResponse
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Audit fields (from AuditEntity)
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Child templates
        public List<TestCaseTemplateResponse> TestCaseTemplates { get; set; } = new();
    }

    public class CreateTestSuiteTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? FolderId { get; set; }

        [StringLength(200)]
        public string? NewFolderName { get; set; } 

        public List<CreateTestCaseTemplateRequest> TestCaseTemplates { get; set; } = new();
    }

    public class UpdateTestSuiteTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? FolderId { get; set; }

        public List<UpdateTestCaseTemplateRequest> TestCaseTemplates { get; set; } = new();
    }

    public class TestCaseTemplateResponse
    {
        public int Id { get; set; }
        public int SuiteTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        public List<TestStepTemplateResponse> TestStepTemplates { get; set; } = new();
    }

    public class CreateTestCaseTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        public List<CreateTestStepTemplateRequest> TestStepTemplates { get; set; } = new();
    }

    public class UpdateTestCaseTemplateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        public List<UpdateTestStepTemplateRequest> TestStepTemplates { get; set; } = new();
    }

    public class TestStepTemplateResponse
    {
        public int Id { get; set; }
        public int TestCaseTemplateId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;
    }

    public class CreateTestStepTemplateRequest
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

    public class UpdateTestStepTemplateRequest
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