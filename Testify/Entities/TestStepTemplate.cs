using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TestStepTemplate
    {
        public int Id { get; set; }
        public int TestCaseTemplateId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;

        // Navigation properties
        public virtual TestCaseTemplate TestCaseTemplate { get; set; } = null!;
    }
}
