using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class TestCaseTemplate
    {
        public int Id { get; set; }
        public int SuiteTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;

        // Navigation properties
        public virtual TestSuiteTemplate SuiteTemplate { get; set; } = null!;
        public virtual ICollection<TestStepTemplate> TestStepTemplates { get; set; } = new List<TestStepTemplate>();
    }
}
