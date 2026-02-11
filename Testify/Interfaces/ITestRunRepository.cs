using Testify.Shared.DTOs.TestPlans;
using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ITestRunRepository
    {
        // === Test Run CRUD ===

        /// <summary>
        /// Get a test run by ID with optional step details
        /// </summary>
        Task<TestRunResponse?> GetByIdAsync(int id, bool includeSteps = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all test runs for a specific test plan
        /// </summary>
        Task<IEnumerable<TestRunResponse>> GetByTestPlanIdAsync(int testPlanId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get test runs by status for a specific test plan
        /// </summary>
        Task<IEnumerable<TestRunResponse>> GetByStatusAsync(int testPlanId, TestRunStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a single test run
        /// </summary>
        Task<TestRunResponse> CreateAsync(CreateTestRunRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing test run (status, comments, executed by)
        /// </summary>
        Task<TestRunResponse> UpdateAsync(int id, UpdateTestRunRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a test run and all its steps
        /// </summary>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        // === Bulk Operations for UC-09 "Start Execution" ===

        /// <summary>
        /// Bulk create test runs when starting test plan execution.
        /// This creates TestRun records with Status = Untested for all test cases.
        /// </summary>
        Task<BulkCreateTestRunsResponse> BulkCreateTestRunsAsync(
            int testPlanId,
            IEnumerable<int> testCaseIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create snapshot of test steps for a test run.
        /// Copies TestSteps → TestRunSteps to freeze test content at execution time.
        /// </summary>
        Task BulkCreateTestRunStepsAsync(
            int testRunId,
            int testCaseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Start execution for a test plan (atomic operation).
        /// 1. Get all test cases from test suites
        /// 2. Create test runs (Status = Untested)
        /// 3. Snapshot test steps into test run steps
        /// Returns the number of test runs created.
        /// </summary>
        Task<int> StartTestPlanExecutionAsync(
            int testPlanId,
            IEnumerable<int> testSuiteIds,
            CancellationToken cancellationToken = default);

        // === Test Run Step Operations ===

        /// <summary>
        /// Get all steps for a specific test run
        /// </summary>
        Task<IEnumerable<TestRunStepResponse>> GetStepsByTestRunIdAsync(int testRunId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific test run step by ID
        /// </summary>
        Task<TestRunStepResponse?> GetStepByIdAsync(int stepId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a test run step (actual result, status)
        /// </summary>
        Task<TestRunStepResponse> UpdateStepAsync(int stepId, UpdateTestRunStepRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple test run steps at once
        /// </summary>
        Task<IEnumerable<TestRunStepResponse>> BatchUpdateStepsAsync(
            IDictionary<int, UpdateTestRunStepRequest> updates,
            CancellationToken cancellationToken = default);

        // === Query & Reporting ===

        /// <summary>
        /// Get test run statistics for a test plan
        /// </summary>
        Task<TestRunStatistics> GetStatisticsByTestPlanIdAsync(int testPlanId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a test plan has any test runs
        /// </summary>
        Task<bool> HasTestRunsAsync(int testPlanId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get test runs executed by a specific user
        /// </summary>
        Task<IEnumerable<TestRunResponse>> GetByExecutedByAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get detailed test run with all steps and attachments
        /// </summary>
        Task<TestRunDetailResponse?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}