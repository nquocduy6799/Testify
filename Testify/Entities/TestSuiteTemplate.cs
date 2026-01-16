using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;

namespace Testify.Entities
{
    public class TestSuiteTemplate : AuditEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ICollection<TestCaseTemplate> TestCaseTemplates { get; set; } = new List<TestCaseTemplate>();
        public virtual ICollection<TestSuite> SourceTestSuites { get; set; } = new List<TestSuite>();
    }
}
