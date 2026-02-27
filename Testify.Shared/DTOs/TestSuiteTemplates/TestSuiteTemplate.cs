using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.Tags;
using Testify.Shared.DTOs.Categories;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.TestTemplates
{
    public class TestSuiteTemplateResponse
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int ViewCount { get; set; }
        public int CloneCount { get; set; }
        public int TotalStarred { get; set; }
        public string? ShareCode { get; set; }

        // Audit fields (from AuditEntity)
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public CategoryResponse? Category { get; set; }
        public List<TagResponse> Tags { get; set; } = new();
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
        public int? CategoryId { get; set; }
        public bool IsPublic { get; set; } = false;

        [StringLength(200)]
        public string? NewFolderName { get; set; }

        public List<int> TagIds { get; set; } = new();
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
        public int? CategoryId { get; set; }
        public bool IsPublic { get; set; }

        public List<int> TagIds { get; set; } = new();
        public List<UpdateTestCaseTemplateRequest> TestCaseTemplates { get; set; } = new();
    }
}