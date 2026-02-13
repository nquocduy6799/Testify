using Testify.Shared.DTOs.Bugs;

namespace Testify.Interfaces
{
    /// <summary>
    /// Repository for bug-specific operations (bugs are KanbanTask entities with Type = TaskType.Bug)
    /// </summary>
    public interface IBugRepository
    {
        /// <summary>
        /// Create a bug from a failed test run step (UC-11)
        /// </summary>
        Task<BugResponse> CreateBugFromTestRunAsync(CreateBugFromTestRunRequest request, string userName, string userId);

        /// <summary>
        /// Get bug by ID with full test run context
        /// </summary>
        Task<BugResponse?> GetBugByIdAsync(int bugId);

        /// <summary>
        /// Get all bugs for a specific project
        /// </summary>
        Task<IEnumerable<BugResponse>> GetBugsByProjectIdAsync(int projectId);

        /// <summary>
        /// Get all bugs for a specific milestone
        /// </summary>
        Task<IEnumerable<BugResponse>> GetBugsByMilestoneIdAsync(int milestoneId);

        /// <summary>
        /// Get all bugs discovered in a specific test run
        /// </summary>
        Task<IEnumerable<BugResponse>> GetBugsByTestRunIdAsync(int testRunId);

        /// <summary>
        /// Link an additional test run step to an existing bug
        /// </summary>
        Task<bool> LinkRunStepToBugAsync(LinkRunStepToBugRequest request, string userName);

        /// <summary>
        /// Remove a test run step link from a bug
        /// </summary>
        Task<bool> UnlinkRunStepFromBugAsync(int bugId, int runStepId);

        /// <summary>
        /// Get bug summary statistics for a project
        /// </summary>
        Task<BugSummary> GetBugSummaryByProjectIdAsync(int projectId);

        /// <summary>
        /// Get bug summary statistics for a milestone
        /// </summary>
        Task<BugSummary> GetBugSummaryByMilestoneIdAsync(int milestoneId);

        /// <summary>
        /// Update bug status (for resolving/closing bugs)
        /// </summary>
        Task<bool> UpdateBugStatusAsync(int bugId, Shared.Enums.KanbanTaskStatus status, string userName, string userId);

        /// <summary>
        /// Get bugs assigned to a specific user
        /// </summary>
        Task<IEnumerable<BugResponse>> GetBugsByAssigneeIdAsync(string assigneeId);

        /// <summary>
        /// Get bugs reported by a specific user
        /// </summary>
        Task<IEnumerable<BugResponse>> GetBugsByReporterIdAsync(string reporterId);
    }
}