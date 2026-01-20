using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class TestCase : AuditEntity
    {
        public int Id { get; set; }
        public int SuiteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Preconditions { get; set; }
        public string? Postconditions { get; set; }
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;
        public DateTime? LastRun { get; set; }

        // Navigation properties
        public virtual TestSuite Suite { get; set; } = null!;
        public virtual ICollection<TestStep> TestSteps { get; set; } = new List<TestStep>();
        public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    }
}
