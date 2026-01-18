using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TestStep
    {
        public int Id { get; set; }
        public int TestCaseId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;

        // Navigation properties
        public virtual TestCase TestCase { get; set; } = null!;
        public virtual ICollection<TestRunStep> TestRunSteps { get; set; } = new List<TestRunStep>();
    }
}
