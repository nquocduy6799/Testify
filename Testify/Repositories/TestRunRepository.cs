using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.DTOs.TestRunStepAttachments;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class TestRunRepository : ITestRunRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserRepository _currentUser;

        public TestRunRepository(ApplicationDbContext context, ICurrentUserRepository currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        #region Test Run CRUD

        public async Task<TestRunResponse?> GetByIdAsync(int id, bool includeSteps = false, CancellationToken cancellationToken = default)
        {
            var query = _context.TestRuns.AsQueryable();

            if (includeSteps)
            {
                query = query.Include(tr => tr.TestRunSteps)
                    .ThenInclude(trs => trs.Attachments);
            }

            var testRun = await query
                .Include(tr => tr.ExecutedBy)
                .Where(tr => tr.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return testRun == null ? null : MapToResponse(testRun);
        }

        public async Task<IEnumerable<TestRunResponse>> GetByTestPlanIdAsync(int testPlanId, CancellationToken cancellationToken = default)
        {
            var testRuns = await _context.TestRuns
                .Include(tr => tr.ExecutedBy)
                .Include(tr => tr.TestCase)
                .Where(tr => tr.TestPlanId == testPlanId)
                .OrderBy(tr => tr.TestCase.Title)
                .ToListAsync(cancellationToken);

            return testRuns.Select(MapToResponse);
        }

        public async Task<IEnumerable<TestRunResponse>> GetByStatusAsync(int testPlanId, TestRunStatus status, CancellationToken cancellationToken = default)
        {
            var testRuns = await _context.TestRuns
                .Include(tr => tr.ExecutedBy)
                .Include(tr => tr.TestCase)
                .Where(tr => tr.TestPlanId == testPlanId && tr.Status == status)
                .OrderBy(tr => tr.TestCase.Title)
                .ToListAsync(cancellationToken);

            return testRuns.Select(MapToResponse);
        }

        public async Task<TestRunResponse> CreateAsync(CreateTestRunRequest request, CancellationToken cancellationToken = default)
        {
            var testRun = new TestRun
            {
                TestPlanId = request.TestPlanId,
                TestCaseId = request.TestCaseId,
                ExecutedByUserId = request.ExecutedByUserId,
                ExecutedAt = request.ExecutedAt,
                Status = request.Status,
                Comments = request.Comments
            };

            _context.TestRuns.Add(testRun);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToResponse(testRun);
        }

        public async Task<TestRunResponse> UpdateAsync(int id, UpdateTestRunRequest request, CancellationToken cancellationToken = default)
        {
            var testRun = await _context.TestRuns.FindAsync([id], cancellationToken);

            if (testRun == null)
                throw new KeyNotFoundException($"TestRun with id {id} not found");

            testRun.ExecutedByUserId = request.ExecutedByUserId;
            testRun.ExecutedAt = request.ExecutedAt;
            testRun.Status = request.Status;
            testRun.Comments = request.Comments;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToResponse(testRun);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var testRun = await _context.TestRuns
                .Include(tr => tr.TestRunSteps)
                .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

            if (testRun == null)
                return false;

            _context.TestRuns.Remove(testRun);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        #endregion

        #region Bulk Operations for UC-09

        public async Task<BulkCreateTestRunsResponse> BulkCreateTestRunsAsync(
            int testPlanId,
            IEnumerable<int> testCaseIds,
            CancellationToken cancellationToken = default)
        {
            var testRuns = testCaseIds.Select(tcId => new TestRun
            {
                TestPlanId = testPlanId,
                TestCaseId = tcId,
                Status = TestRunStatus.Untested,
                ExecutedAt = null,
                ExecutedByUserId = null,
                Comments = null
            }).ToList();

            await _context.TestRuns.AddRangeAsync(testRuns, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new BulkCreateTestRunsResponse
            {
                TotalCreated = testRuns.Count,
                TestRunIds = testRuns.Select(tr => tr.Id).ToList()
            };
        }

        public async Task BulkCreateTestRunStepsAsync(
            int testRunId,
            int testCaseId,
            CancellationToken cancellationToken = default)
        {
            // Get all test steps for this test case
            var testSteps = await _context.TestSteps
                .Where(ts => ts.TestCaseId == testCaseId)
                .OrderBy(ts => ts.StepNumber)
                .ToListAsync(cancellationToken);

            // Create snapshot of test steps
            var testRunSteps = testSteps.Select(ts => new TestRunStep
            {
                RunId = testRunId,
                StepId = ts.Id, // Reference to original step
                StepNumber = ts.StepNumber,
                Action = ts.Action,
                TestData = ts.TestData,
                ExpectedResult = ts.ExpectedResult,
                Status = TestStepStatus.Pass, // Default status
                ActualResult = null
            }).ToList();

            // Set audit information
            var userName = _currentUser.UserName ?? "System";
            foreach (var step in testRunSteps)
            {
                step.MarkAsCreated(userName);
            }

            await _context.TestRunSteps.AddRangeAsync(testRunSteps, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> StartTestPlanExecutionAsync(
            int testPlanId,
            IEnumerable<int> testSuiteIds,
            CancellationToken cancellationToken = default)
        {
            // Begin transaction for atomicity
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Verify test plan exists and is in Draft status
                var testPlan = await _context.TestPlans.FindAsync([testPlanId], cancellationToken);
                if (testPlan == null)
                    throw new KeyNotFoundException($"TestPlan with id {testPlanId} not found");

                if (testPlan.Status != TestPlanStatus.Draft)
                    throw new InvalidOperationException("Only Draft test plans can be started");

                // 2. Get all test cases from the selected test suites
                var testCaseIds = await _context.TestCases
                    .Where(tc => testSuiteIds.Contains(tc.SuiteId) && !tc.IsDeleted)
                    .Select(tc => tc.Id)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (!testCaseIds.Any())
                    throw new InvalidOperationException("No test cases found in the selected test suites");

                // 3. Bulk create TestRuns with Status = Untested
                var bulkResponse = await BulkCreateTestRunsAsync(testPlanId, testCaseIds, cancellationToken);

                // 4. For each TestRun, create snapshot of TestSteps → TestRunSteps
                var testRuns = await _context.TestRuns
                    .Where(tr => bulkResponse.TestRunIds.Contains(tr.Id))
                    .ToListAsync(cancellationToken);

                foreach (var testRun in testRuns)
                {
                    await BulkCreateTestRunStepsAsync(testRun.Id, testRun.TestCaseId, cancellationToken);
                }

                // 5. Update test plan status to InProgress
                testPlan.Status = TestPlanStatus.InProgress;
                testPlan.StartedAt = DateTimeHelper.GetVietnamTime();

                var userName = _currentUser.UserName ?? "System";
                testPlan.MarkAsUpdated(userName);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return bulkResponse.TotalCreated;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        #endregion

        #region Test Run Step Operations

        public async Task<IEnumerable<TestRunStepResponse>> GetStepsByTestRunIdAsync(int testRunId, CancellationToken cancellationToken = default)
        {
            var steps = await _context.TestRunSteps
                .Include(trs => trs.Attachments)
                .Where(trs => trs.RunId == testRunId && !trs.IsDeleted)
                .OrderBy(trs => trs.StepNumber)
                .ToListAsync(cancellationToken);

            return steps.Select(MapStepToResponse);
        }

        public async Task<TestRunStepResponse?> GetStepByIdAsync(int stepId, CancellationToken cancellationToken = default)
        {
            var step = await _context.TestRunSteps
                .Include(trs => trs.Attachments)
                .Where(trs => trs.Id == stepId && !trs.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            return step == null ? null : MapStepToResponse(step);
        }

        public async Task<TestRunStepResponse> UpdateStepAsync(int stepId, UpdateTestRunStepRequest request, CancellationToken cancellationToken = default)
        {
            var step = await _context.TestRunSteps.FindAsync([stepId], cancellationToken);

            if (step == null || step.IsDeleted)
                throw new KeyNotFoundException($"TestRunStep with id {stepId} not found");

            step.Status = request.Status;
            step.ActualResult = request.ActualResult;

            var userName = _currentUser.UserName ?? "System";
            step.MarkAsUpdated(userName);

            await _context.SaveChangesAsync(cancellationToken);

            return MapStepToResponse(step);
        }

        public async Task<IEnumerable<TestRunStepResponse>> BatchUpdateStepsAsync(
            IDictionary<int, UpdateTestRunStepRequest> updates,
            CancellationToken cancellationToken = default)
        {
            var stepIds = updates.Keys.ToList();
            var steps = await _context.TestRunSteps
                .Where(trs => stepIds.Contains(trs.Id) && !trs.IsDeleted)
                .ToListAsync(cancellationToken);

            var userName = _currentUser.UserName ?? "System";

            foreach (var step in steps)
            {
                if (updates.TryGetValue(step.Id, out var request))
                {
                    step.Status = request.Status;
                    step.ActualResult = request.ActualResult;
                    step.MarkAsUpdated(userName);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return steps.Select(MapStepToResponse);
        }

        #endregion

        #region Query & Reporting

        public async Task<TestRunStatistics> GetStatisticsByTestPlanIdAsync(int testPlanId, CancellationToken cancellationToken = default)
        {
            var testRuns = await _context.TestRuns
                .Where(tr => tr.TestPlanId == testPlanId)
                .ToListAsync(cancellationToken);

            var stats = new TestRunStatistics
            {
                TotalTests = testRuns.Count,
                Untested = testRuns.Count(tr => tr.Status == TestRunStatus.Untested),
                Passed = testRuns.Count(tr => tr.Status == TestRunStatus.Passed),
                Failed = testRuns.Count(tr => tr.Status == TestRunStatus.Failed),
                Blocked = testRuns.Count(tr => tr.Status == TestRunStatus.Blocked),
                Skipped = testRuns.Count(tr => tr.Status == TestRunStatus.Skipped)
            };

            return stats;
        }

        public async Task<bool> HasTestRunsAsync(int testPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.TestRuns
                .AnyAsync(tr => tr.TestPlanId == testPlanId, cancellationToken);
        }

        public async Task<IEnumerable<TestRunResponse>> GetByExecutedByAsync(string userId, CancellationToken cancellationToken = default)
        {
            var testRuns = await _context.TestRuns
                .Include(tr => tr.ExecutedBy)
                .Include(tr => tr.TestCase)
                .Where(tr => tr.ExecutedByUserId == userId)
                .OrderByDescending(tr => tr.ExecutedAt)
                .ToListAsync(cancellationToken);

            return testRuns.Select(MapToResponse);
        }

        public async Task<TestRunDetailResponse?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var testRun = await _context.TestRuns
                .Include(tr => tr.TestCase)
                .Include(tr => tr.ExecutedBy)
                .Include(tr => tr.TestRunSteps.OrderBy(s => s.StepNumber))
                    .ThenInclude(trs => trs.Attachments)
                .Where(tr => tr.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (testRun == null)
                return null;

            return new TestRunDetailResponse
            {
                Id = testRun.Id,
                TestPlanId = testRun.TestPlanId,
                TestCaseId = testRun.TestCaseId,
                ExecutedByUsername = testRun.ExecutedBy?.UserName,
                ExecutedAt = testRun.ExecutedAt,
                Status = testRun.Status,
                Comments = testRun.Comments,
                CreatedAt = DateTimeHelper.GetVietnamTime(),
                CreatedBy = "System",
                TestCaseTitle = testRun.TestCase.Title,
                Steps = testRun.TestRunSteps
                    .Where(s => !s.IsDeleted)
                    .Select(MapStepToResponse)
                    .ToList()
            };
        }

        #endregion

        #region Mapping Methods
        private static TestRunResponse MapToResponse(TestRun testRun)
        {
            return new TestRunResponse
            {
                Id = testRun.Id,
                TestPlanId = testRun.TestPlanId,
                TestCaseId = testRun.TestCaseId,
                ExecutedByUsername = testRun.ExecutedBy?.UserName,
                ExecutedAt = testRun.ExecutedAt,
                Status = testRun.Status,
                Comments = testRun.Comments,
                CreatedAt = testRun.CreatedAt,  
                CreatedBy = testRun.CreatedBy    
            };
        }

        private static TestRunStepResponse MapStepToResponse(TestRunStep step)
        {
            return new TestRunStepResponse
            {
                Id = step.Id,
                TestRunId = step.RunId,
                OriginalStepId = step.StepId,
                StepNumber = step.StepNumber,
                Action = step.Action,
                TestData = step.TestData,
                ExpectedResult = step.ExpectedResult,
                Status = step.Status,
                ActualResult = step.ActualResult,
                Attachments = step.Attachments
                    .Select(a => new TestRunStepAttachmentResponse
                    {
                        Id = a.Id,
                        RunStepId = a.RunStepId,
                        FileName = a.FileName,
                        FileUrl = a.FileUrl,
                        FileSize = a.FileSize,
                        ContentType = a.ContentType,
                        UpdatedAt = a.UpdatedAt,
                        UpdatedBy = a.UpdatedBy
                    })
                    .ToList()
            };
        }

        #endregion
    }
}


