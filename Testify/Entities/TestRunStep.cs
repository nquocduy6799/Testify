using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class TestRunStep
    {
        public int RunStepId { get; set; }
        public int RunId { get; set; }
        public int? StepId { get; set; }

        // Snapshot data
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;

        // Execution data
        public TestStepStatus Status { get; set; } = TestStepStatus.Pass;
        public string? ActualResult { get; set; }

        // Navigation properties
        public virtual TestRun Run { get; set; } = null!;
        public virtual TestStep? Step { get; set; }
        public virtual ICollection<TaskLinkedRunStep> LinkedTasks { get; set; } = new List<TaskLinkedRunStep>();
        public virtual ICollection<TestRunStepAttachment> Attachments { get; set; } = new List<TestRunStepAttachment>();
    }
}
