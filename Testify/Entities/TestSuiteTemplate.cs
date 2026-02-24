using System.ComponentModel.DataAnnotations.Schema;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class TestSuiteTemplate : AuditEntity
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public int? CategoryId { get; set; }
        public required string Name { get; set; }
        public bool IsPublic { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public int CloneCount { get; set; } = 0;
        public int TotalStarred { get; set; } = 0;
        public string? ShareCode { get; set; }
        public string? Description { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual TemplateFolder? Folder { get; set; }
        public virtual TemplateCategory? Category { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<TestCaseTemplate> TestCaseTemplates { get; set; } = new List<TestCaseTemplate>();
        public virtual ICollection<TestSuiteTemplateTag> Tags { get; set; } = new List<TestSuiteTemplateTag>();
        public virtual ICollection<TemplateReview> Reviews { get; set; } = new List<TemplateReview>();
    }
}



