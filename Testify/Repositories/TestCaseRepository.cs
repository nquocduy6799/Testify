using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Repositories
{
    public class TestCaseRepository : ITestCaseRepository
    {
        private readonly ApplicationDbContext _context;

        public TestCaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestCaseResponse?> GetTestCaseByIdAsync(int id)
        {
            var testCase = await _context.TestCases
                .Where(c => !c.IsDeleted && c.Id == id)
                .Include(c => c.TestSteps)
                .FirstOrDefaultAsync();

            return testCase is null ? null : MapToResponse(testCase);
        }

        public async Task<TestCaseResponse> CreateTestCaseAsync(int suiteId, CreateTestCaseRequest request, string userName)
        {
            var testCase = new TestCase
            {
                SuiteId = suiteId,
                Title = request.Title,
                Preconditions = request.Preconditions,
                Postconditions = request.Postconditions,
                Priority = request.Priority
            };
            testCase.MarkAsCreated(userName);

            foreach (var step in request.TestSteps)
            {
                testCase.TestSteps.Add(new TestStep
                {
                    StepNumber = step.StepNumber,
                    Action = step.Action,
                    TestData = step.TestData,
                    ExpectedResult = step.ExpectedResult
                });
            }

            _context.TestCases.Add(testCase);
            await _context.SaveChangesAsync();

            // Reload to get generated IDs
            return (await GetTestCaseByIdAsync(testCase.Id))!;
        }

        public async Task<bool> UpdateTestCaseAsync(int id, UpdateTestCaseRequest request, string userName)
        {
            var testCase = await _context.TestCases
                .Where(c => !c.IsDeleted && c.Id == id)
                .Include(c => c.TestSteps)
                .FirstOrDefaultAsync();

            if (testCase is null) return false;

            testCase.Title = request.Title;
            testCase.Preconditions = request.Preconditions;
            testCase.Postconditions = request.Postconditions;
            testCase.Priority = request.Priority;
            testCase.MarkAsUpdated(userName);

            // Replace all steps (same strategy as template)
            _context.TestSteps.RemoveRange(testCase.TestSteps);

            foreach (var step in request.TestSteps)
            {
                testCase.TestSteps.Add(new TestStep
                {
                    StepNumber = step.StepNumber,
                    Action = step.Action,
                    TestData = step.TestData,
                    ExpectedResult = step.ExpectedResult
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestCaseAsync(int id)
        {
            var testCase = await _context.TestCases
                .Where(c => !c.IsDeleted && c.Id == id)
                .FirstOrDefaultAsync();

            if (testCase is null) return false;

            testCase.MarkAsDeleted("system");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCaseTitleExistsAsync(int suiteId, string title, int? excludeCaseId = null)
        {
            return await _context.TestCases
                .AnyAsync(c => !c.IsDeleted
                    && c.SuiteId == suiteId
                    && c.Title.ToLower() == title.ToLower()
                    && (!excludeCaseId.HasValue || c.Id != excludeCaseId.Value));
        }

        private static TestCaseResponse MapToResponse(TestCase c)
        {
            return new TestCaseResponse
            {
                Id = c.Id,
                SuiteId = c.SuiteId,
                Title = c.Title,
                Preconditions = c.Preconditions,
                Postconditions = c.Postconditions,
                Priority = c.Priority,
                LastRun = c.LastRun,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy,
                TestSteps = c.TestSteps
                    .OrderBy(st => st.StepNumber)
                    .Select(st => new TestStepResponse
                    {
                        Id = st.Id,
                        TestCaseId = st.TestCaseId,
                        StepNumber = st.StepNumber,
                        Action = st.Action,
                        TestData = st.TestData,
                        ExpectedResult = st.ExpectedResult
                    }).ToList()
            };
        }
    }
}
