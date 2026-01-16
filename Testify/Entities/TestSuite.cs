using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TestSuite : AuditEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? FolderId { get; set; }
        public int? SourceTemplateId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ProjectFolder? Folder { get; set; }
        public virtual TestSuiteTemplate? SourceTemplate { get; set; }
        public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
        public virtual ICollection<TestPlanSuite> TestPlanSuites { get; set; } = new List<TestPlanSuite>();
    }
}
