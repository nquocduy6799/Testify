using Testify.Shared.DTOs.Bugs;

namespace Testify.Client.Interfaces
{
    public interface IBugService
    {
        #region Bug CRUD Operations

        /// <summary>
        /// Creates a bug from a failed test run with steps to reproduce and linked run steps.
        /// </summary>
        /// <param name="request">The bug creation request containing test run context</param>
        /// <returns>The created bug response, or null if creation failed</returns>
        Task<BugResponse?> CreateBugFromTestRunAsync(CreateBugFromTestRunRequest request);

        /// <summary>
        /// Gets a bug by ID.
        /// </summary>
        /// <param name="id">The bug ID</param>
        /// <returns>The bug response, or null if not found</returns>
        Task<BugResponse?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all bugs for a milestone.
        /// </summary>
        /// <param name="milestoneId">The milestone ID</param>
        /// <returns>List of bugs in the milestone</returns>
        Task<List<BugResponse>> GetByMilestoneIdAsync(int milestoneId);

        /// <summary>
        /// Gets all bugs for a project.
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>List of bugs in the project</returns>
        Task<List<BugResponse>> GetByProjectIdAsync(int projectId);

        #endregion

        #region Bug Update Operations

        /// <summary>
        /// Updates an existing bug.
        /// </summary>
        /// <param name="id">The bug ID to update</param>
        /// <param name="request">The update request</param>
        /// <returns>True if update succeeded, false otherwise</returns>
        Task<bool> UpdateAsync(int id, UpdateBugRequest request);

        #endregion

        #region Bug Delete Operations

        /// <summary>
        /// Deletes a bug by ID.
        /// </summary>
        /// <param name="id">The bug ID to delete</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        Task<bool> DeleteAsync(int id);

        #endregion
    }
}