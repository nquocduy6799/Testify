using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface ITestRunService
    {
        #region Test Run CRUD Operations

        /// <summary>
        /// Get a test run by ID
        /// </summary>
        Task<TestRunResponse?> GetByIdAsync(int id, bool includeSteps = false);

        /// <summary>
        /// Get detailed test run with all steps and attachments
        /// </summary>
        Task<TestRunDetailResponse?> GetDetailedByIdAsync(int id);

        /// <summary>
        /// Get all test runs for a test plan
        /// </summary>
        Task<List<TestRunResponse>> GetByTestPlanIdAsync(int testPlanId);

        /// <summary>
        /// Get test runs by status for a test plan
        /// </summary>
        Task<List<TestRunResponse>> GetByStatusAsync(int testPlanId, TestRunStatus status);

        /// <summary>
        /// Get test runs executed by a specific user
        /// </summary>
        Task<List<TestRunResponse>> GetByExecutedByAsync(string userId);

        /// <summary>
        /// Create a new test run
        /// </summary>
        Task<TestRunResponse> CreateAsync(CreateTestRunRequest request);

        /// <summary>
        /// Update an existing test run
        /// </summary>
        Task<bool> UpdateAsync(int id, UpdateTestRunRequest request);

        /// <summary>
        /// Delete a test run
        /// </summary>
        Task<bool> DeleteAsync(int id);

        #endregion

        #region UC-09: Start Test Plan Execution

        /// <summary>
        /// Start test plan execution (UC-09)
        /// Creates test runs and snapshots test steps for all test cases in the selected suites
        /// </summary>
        Task<StartExecutionResponse?> StartExecutionAsync(int testPlanId, List<int> testSuiteIds);

        /// <summary>
        /// Bulk create test runs without starting execution
        /// </summary>
        Task<BulkCreateTestRunsResponse?> BulkCreateTestRunsAsync(int testPlanId, List<int> testCaseIds);

        #endregion

        #region Test Run Step Operations

        /// <summary>
        /// Get all steps for a test run
        /// </summary>
        Task<List<TestRunStepResponse>> GetStepsByTestRunIdAsync(int testRunId);

        /// <summary>
        /// Get a specific test run step
        /// </summary>
        Task<TestRunStepResponse?> GetStepByIdAsync(int stepId);

        /// <summary>
        /// Update a test run step (log test results)
        /// </summary>
        Task<bool> UpdateStepAsync(int stepId, UpdateTestRunStepRequest request);

        /// <summary>
        /// Batch update multiple test run steps
        /// </summary>
        Task<bool> BatchUpdateStepsAsync(Dictionary<int, UpdateTestRunStepRequest> updates);

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get test run statistics for a test plan
        /// </summary>
        Task<TestRunStatistics?> GetStatisticsByTestPlanIdAsync(int testPlanId);

        /// <summary>
        /// Check if a test plan has any test runs
        /// </summary>
        Task<bool> HasTestRunsAsync(int testPlanId);

        #endregion
    }
}