using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Bugs
{
    /// <summary>
    /// Request DTO for creating a bug from a failed test run step.
    /// Inherits from CreateKanbanTaskRequest but adds bug-specific context.
    /// </summary>
    public class CreateBugFromTestRunRequest
    {
        [Required(ErrorMessage = "Milestone is required.")]
        public int MilestoneId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public BugSeverity Severity { get; set; } = BugSeverity.Medium;

        public string? AssigneeId { get; set; }

        // Bug-specific properties for UC-11
        /// <summary>
        /// The test run step IDs where the bug was found (for auto-populating steps to reproduce)
        /// </summary>
        [Required(ErrorMessage = "At least one failed test run step is required.")]
        public List<int> FailedRunStepIds { get; set; } = new();

        /// <summary>
        /// The test run ID where the bug was discovered
        /// </summary>
        public int? TestRunId { get; set; }

        /// <summary>
        /// Auto-generated steps to reproduce from TestRunStep data
        /// Format: "Step {StepNumber}: {Action}\nTest Data: {TestData}\nExpected: {ExpectedResult}\nActual: {ActualResult}"
        /// </summary>
        public string StepsToReproduce { get; set; } = string.Empty;
    }


    /// <summary>
    /// Response DTO for bug with additional test run linking context
    /// </summary>
    public class BugResponse
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StepsToReproduce { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }
        public KanbanTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        // Assignee information
        public string? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public string? AssigneeAvatarUrl { get; set; }

        // Bug origin
        public int? TestCaseId { get; set; }
        public string? TestCaseName { get; set; }
        public int? TestRunId { get; set; }
        public string? ReportedBy { get; set; }
        public string? ReportedByName { get; set; }

        // Linked test run steps
        public List<LinkedRunStepInfo> LinkedRunSteps { get; set; } = new();

        // Attachments (screenshots)
        public List<TestRunStepAttachmentResponse> Attachments { get; set; } = new();
    }


    public class UpdateBugRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters.")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public BugSeverity Severity { get; set; } = BugSeverity.Medium;
        public string? AssigneeId { get; set; }
    }

    /// <summary>
    /// Information about linked test run steps
    /// </summary>
    public class LinkedRunStepInfo
    {
        public int RunStepId { get; set; }
        public int StepNumber { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
        public string? ActualResult { get; set; }
        public string? TestData { get; set; }
        public DateTime LinkedAt { get; set; }
    }

    public class TestRunStepAttachmentResponse 
    {
        public int Id { get; set; }
        public int RunStepId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to link additional test run steps to an existing bug
    /// </summary>
    public class LinkRunStepToBugRequest
    {
        [Required]
        public int BugId { get; set; }

        [Required]
        public int RunStepId { get; set; }
    }

    /// <summary>
    /// Summary statistics for bugs in a project/milestone
    /// </summary>
    public class BugSummary
    {
        public int TotalBugs { get; set; }
        public int OpenBugs { get; set; }
        public int InProgressBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public int CriticalBugs { get; set; }
        public int HighPriorityBugs { get; set; }
        public Dictionary<string, int> BugsByStatus { get; set; } = new();
        public Dictionary<string, int> BugsByPriority { get; set; } = new();
    }
}