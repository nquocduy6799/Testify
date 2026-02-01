using System.ComponentModel.DataAnnotations.Schema;
using Testify.Data;

namespace Testify.Entities
{
    public class TestSuiteTemplate : AuditEntity
    {
        public int Id { get; set; }
        public int? FolderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual TemplateFolder? Folder { get; set; }
        public virtual ICollection<TestCaseTemplate> TestCaseTemplates { get; set; } = new List<TestCaseTemplate>();
        public virtual ICollection<TestSuite> SourceTestSuites { get; set; } = new List<TestSuite>();
    }
}
