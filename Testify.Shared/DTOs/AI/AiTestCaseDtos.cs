using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.AI
{
    /// <summary>
    /// Request to generate test cases via AI.
    /// </summary>
    public class AiGenerateTestCasesRequest
    {
        /// <summary>Feature / module description for the AI to base test cases on.</summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>Number of test cases to generate (1-20).</summary>
        public int Count { get; set; } = 5;

        /// <summary>Default priority for generated cases.</summary>
        public TestCasePriority DefaultPriority { get; set; } = TestCasePriority.Medium;

        /// <summary>Optional: suite name for extra context.</summary>
        public string? SuiteName { get; set; }

        /// <summary>Optional: suite description for extra context.</summary>
        public string? SuiteDescription { get; set; }

        /// <summary>Optional: titles of existing test cases to avoid duplication.</summary>
        public List<string>? ExistingTestCaseTitles { get; set; }
    }

    /// <summary>
    /// Response wrapping AI-generated test cases.
    /// </summary>
    public class AiGenerateTestCasesResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<TestCaseFormResult> TestCases { get; set; } = new();

        /// <summary>Remaining AI generation uses after this request (for free tier).</summary>
        public int RemainingUses { get; set; }
    }

    /// <summary>
    /// Response for checking current AI usage quota.
    /// </summary>
    public class AiUsageResponse
    {
        /// <summary>How many times the user has used AI generation.</summary>
        public int UsedCount { get; set; }

        /// <summary>Maximum allowed uses for the current tier.</summary>
        public int MaxCount { get; set; }

        /// <summary>Number of remaining uses.</summary>
        public int RemainingUses => MaxCount - UsedCount;

        /// <summary>Whether the user can still generate.</summary>
        public bool CanGenerate => RemainingUses > 0;
    }
}
