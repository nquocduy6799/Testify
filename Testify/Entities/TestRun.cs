using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class TestRun
    {
        public int RunId { get; set; }
        public int PlanId { get; set; }
        public int TestCaseId { get; set; }
        public string? ExecutedByUserId { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
        public string? Comments { get; set; }

        // Navigation properties
        public virtual TestPlan Plan { get; set; } = null!;
        public virtual TestCase TestCase { get; set; } = null!;
        public virtual ApplicationUser? ExecutedBy { get; set; }
        public virtual ICollection<TestRunStep> TestRunSteps { get; set; } = new List<TestRunStep>();
    }
}
