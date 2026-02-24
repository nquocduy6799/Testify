using Testify.Shared.Enums;

namespace Testify.Shared.DTOs
{
    /// <summary>
    /// Shared form result used by TestCaseForm component.
    /// Both Template and Project-level test case creation flows
    /// emit this model, and the parent page converts it to the
    /// appropriate DTO (CreateTestCaseTemplateRequest or CreateTestCaseRequest).
    /// </summary>
    public class TestCaseFormResult
    {
        public string Title { get; set; } = string.Empty;
        public string? Preconditions { get; set; }
        public string? Postconditions { get; set; }
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;
        public List<TestStepFormResult> Steps { get; set; } = new();
    }

    public class TestStepFormResult
    {
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;
    }
}
