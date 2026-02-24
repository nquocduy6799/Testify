using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Testify.Shared.DTOs.TestRunStepAttachments;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Shared.DTOs.TestRuns
{
    public class TestRunResponse
    {
        public int Id { get; set; }
        [Required]
        public int TestPlanId { get; set; }
        [Required]
        public int TestCaseId { get; set; }
        public string? ExecutedByUsername { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class TestRunDetailResponse : TestRunResponse
    {
        public string TestCaseTitle { get; set; } = string.Empty;
        public ICollection<TestRunStepResponse> Steps { get; set; } = new List<TestRunStepResponse>();
    }

    public class CreateTestRunRequest
    {
        [Required]
        public int TestPlanId { get; set; }
        [Required]
        public int TestCaseId { get; set; }
        public string? ExecutedByUserId { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    public class UpdateTestRunRequest
    {
        public string? ExecutedByUserId { get; set; }
        public DateTime? ExecutedAt { get; set; }
        [Required]
        public TestRunStatus Status { get; set; }
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }


    public class TestRunStepResponse
    {
        public int Id { get; set; }
        [Required]
        public int TestRunId { get; set; } 
        public int? OriginalStepId { get; set; } 

        // Snapshot data (frozen at execution start)
        public int StepNumber { get; set; }
        [Required]
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        [Required]
        public string ExpectedResult { get; set; } = string.Empty;

        public TestStepStatus Status { get; set; } = TestStepStatus.Pass;
        public string? ActualResult { get; set; }

        public ICollection<TestRunStepAttachmentResponse> Attachments { get; set;  } = new List<TestRunStepAttachmentResponse>();
    }

    public class UpdateTestRunStepRequest
    {
        [Required]
        public TestStepStatus Status { get; set; }
        [MaxLength(2000)]
        public string? ActualResult { get; set; }
    }

    public class BulkCreateTestRunsResponse
    {
        public int TotalCreated { get; set; }
        public List<int> TestRunIds { get; set; } = new();
    }


    public class TestCaseResponse
    {
        public int Id { get; set; }
        public int SuiteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Preconditions { get; set; }
        public string? Postconditions { get; set; }
        public TestCasePriority Priority { get; set; } = TestCasePriority.Medium;
        public DateTime? LastRun { get; set; }

        // Navigation properties
        public virtual ICollection<TestStepResponse> TestSteps { get; set; } = new List<TestStepResponse>();
    }

    public class TestStepResponse
    {
        public int Id { get; set; }
        public int TestCaseId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TestData { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;
    }

    // Statistics for a test run
    public class TestRunStatistics
    {
        public int TotalTests { get; set; }
        public int Untested { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Blocked { get; set; }
        public int Skipped { get; set; }
        public decimal PassRate => TotalTests > 0 ? (decimal)Passed / TotalTests * 100 : 0;
        public decimal CompletionRate => TotalTests > 0 ? (decimal)(TotalTests - Untested) / TotalTests * 100 : 0;
    }


    #region Supporting DTOs (should match controller DTOs)

    public class StartExecutionRequest
    {
        public int TestPlanId { get; set; }
        public List<int> TestSuiteIds { get; set; } = new();
    }

    public class StartExecutionResponse
    {
        public int TestPlanId { get; set; }
        public int TotalTestRunsCreated { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BulkCreateTestRunsRequest
    {
        [Required]
        public int TestPlanId { get; set; }
        [Required]
        [MinLength(1)]
        public List<int> TestCaseIds { get; set; } = new();
    }

    #endregion

    public class TestRunStepAttachmentInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public int RunStepId { get; set; }
    }
}



































//public class TestRunResponse
//{
//    public int Id { get; set; }
//    public int PlanId { get; set; }
//    public int TestCaseId { get; set; }
//    public string? ExecutedByUsername { get; set; }
//    public DateTime? ExecutedAt { get; set; }
//    public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
//    public string? Comments { get; set; }
//    public DateTime CreatedAt { get; set; }
//    public string CreatedBy { get; set; } = string.Empty;
//}




//public class CreateTestRunRequest
//{
//    public int PlanId { get; set; }
//    public int TestCaseId { get; set; }
//    public string? ExecutedByUserId { get; set; }
//    public DateTime? ExecutedAt { get; set; }
//    public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
//    public string? Comments { get; set; }
//}

//public class UpdateTestRunRequest
//{
//    public int PlanId { get; set; }
//    public int TestCaseId { get; set; }
//    public string? ExecutedByUserId { get; set; }
//    public DateTime? ExecutedAt { get; set; }
//    public TestRunStatus Status { get; set; } = TestRunStatus.Untested;
//    public string? Comments { get; set; }
//}

//public class TestRunStepResponse
//{
//    public int Id { get; set; }
//    public int RunId { get; set; }
//    public int? StepId { get; set; }

//    // Snapshot data
//    public int StepNumber { get; set; }
//    public string Action { get; set; } = string.Empty;
//    public string? TestData { get; set; }
//    public string ExpectedResult { get; set; } = string.Empty;

//    // Execution data
//    public TestStepStatus Status { get; set; } = TestStepStatus.Pass;
//    public string? ActualResult { get; set; }

//    //Audit data
//    public DateTime CreatedAt { get; set; }
//    public string CreatedBy { get; set; } = string.Empty;
//    public DateTime? UpdatedAt { get; set; }
//    public string? UpdatedBy { get; set; }

//    // Navigation properties
//    public virtual ICollection<TestRunStepAttachmentResponse> Attachments { get; set; } = new List<TestRunStepAttachmentResponse>();
//}


//public class CreateTestRunStepRequest
//{
//    public int RunId { get; set; }
//    public int? StepId { get; set; }

//    // Snapshot data
//    public int StepNumber { get; set; }
//    public string Action { get; set; } = string.Empty;
//    public string? TestData { get; set; }
//    public string ExpectedResult { get; set; } = string.Empty;

//    // Execution data
//    public TestStepStatus Status { get; set; } = TestStepStatus.Pass;
//    public string? ActualResult { get; set; }
//}


//public class UpdateTestRunStepRequest
//{
//    [Required]
//    public TestStepStatus Status { get; set; }
//    [MaxLength(2000)]
//    public string? ActualResult { get; set; }
//}