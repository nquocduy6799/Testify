using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class TestPlanRepository : ITestPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public TestPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestPlanResponse> CreateTestPlanAsync(CreateTestPlanRequest request, string userName, string userId)
        {
            var testPlan = new TestPlan
            {
                ProjectId = request.ProjectId,
                TaskId = request.TaskId,
                MilestoneId = request.MilestoneId,
                Scope = request.Scope,
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority
            };

            testPlan.MarkAsCreated(userName);

            _context.TestPlans.Add(testPlan);
            await _context.SaveChangesAsync();

            await UpdateTestPlanSuitesAsync(testPlan.Id, request.TestSuites);

            return await GetTestPlanByIdInternalAsync(testPlan.Id)
                   ?? MapToResponse(testPlan, new List<TestSuiteResponse>());
        }

        public async Task<bool> DeleteTestPlanAsync(int id, string userName)
        {
            var testPlan = await _context.TestPlans.FindAsync(id);

            if (testPlan == null || testPlan.IsDeleted || testPlan.Status != TestPlanStatus.Draft)
                return false;

            var relatedSuites = _context.TestPlanSuites.Where(tps => tps.TestPlanId == id);
            _context.TestPlanSuites.RemoveRange(relatedSuites);

            _context.TestPlans.Remove(testPlan);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TestPlanResponse>> GetAllTestPlansAsync(int projectId)
        {
            var plans = await _context.TestPlans
                .Include(tp => tp.TestPlanSuites)
                    .ThenInclude(tps => tps.TestSuite)
                .Where(tp => tp.ProjectId == projectId && !tp.IsDeleted)
                .ToListAsync();

            return plans.Select(p => MapToResponse(p)).ToList();
        }

        public async Task<TestPlanResponse?> GetTestPlanByIdAsync(int id)
        {
            return await GetTestPlanByIdInternalAsync(id);
        }

        public async Task<bool> UpdateTestPlanAsync(int id, UpdateTestPlanRequest request, string userName)
        {
            var testPlan = await _context.TestPlans
                .Include(tp => tp.TestPlanSuites)
                .FirstOrDefaultAsync(tp => tp.Id == id && !tp.IsDeleted);

            if (testPlan == null)
                return false;

            testPlan.ProjectId = request.ProjectId;
            testPlan.TaskId = request.TaskId;
            testPlan.MilestoneId = request.MilestoneId;
            testPlan.Scope = request.Scope;
            testPlan.Name = request.Name;
            testPlan.Description = request.Description;
            testPlan.Status = request.Status;
            testPlan.Priority = request.Priority;
            testPlan.MarkAsUpdated(userName);

            try
            {
                await _context.SaveChangesAsync();
                await UpdateTestPlanSuitesAsync(testPlan.Id, request.TestSuites);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TestPlanExistsAsync(id))
                    return false;

                throw;
            }
        }

        private async Task<TestPlanResponse?> GetTestPlanByIdInternalAsync(int id)
        {
            var plan = await _context.TestPlans
                .Include(tp => tp.TestPlanSuites)
                    .ThenInclude(tps => tps.TestSuite)
                .FirstOrDefaultAsync(tp => tp.Id == id && !tp.IsDeleted);

            return plan == null ? null : MapToResponse(plan);
        }

        private async Task UpdateTestPlanSuitesAsync(int testPlanId, List<TestSuiteResponse> suites)
        {
            var existingSuites = await _context.TestPlanSuites
                .Where(tps => tps.TestPlanId == testPlanId)
                .ToListAsync();

            _context.TestPlanSuites.RemoveRange(existingSuites);

            var suiteIds = suites?
                .Select(s => s.Id)
                .Distinct()
                .ToList() ?? new List<int>();

            if (suiteIds.Count == 0)
            {
                await _context.SaveChangesAsync();
                return;
            }

            var validSuiteIds = await _context.TestSuites
                .Where(ts => suiteIds.Contains(ts.Id) && !ts.IsDeleted)
                .Select(ts => ts.Id)
                .ToListAsync();

            var planSuites = validSuiteIds.Select(id => new TestPlanSuite
            {
                TestPlanId = testPlanId,
                TestSuiteId = id,
                AddedAt = DateTimeHelper.GetVietnamTime()
            });

            await _context.TestPlanSuites.AddRangeAsync(planSuites);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> TestPlanExistsAsync(int id)
        {
            return await _context.TestPlans.AnyAsync(tp => tp.Id == id && !tp.IsDeleted);
        }

        private static TestPlanResponse MapToResponse(TestPlan plan)
        {
            var suites = plan.TestPlanSuites
                .Where(tps => !tps.TestSuite.IsDeleted)
                .Select(tps => MapToResponse(tps.TestSuite))
                .ToList();

            return MapToResponse(plan, suites);
        }

        private static TestPlanResponse MapToResponse(TestPlan plan, List<TestSuiteResponse> suites)
        {
            return new TestPlanResponse
            {
                Id = plan.Id,
                ProjectId = plan.ProjectId,
                TaskId = plan.TaskId,
                MilestoneId = plan.MilestoneId,
                Scope = plan.Scope,
                Name = plan.Name,
                Description = plan.Description,
                Status = plan.Status,
                Outcome = plan.Outcome,
                StartedAt = plan.StartedAt,
                CompletedAt = plan.CompletedAt,
                Priority = plan.Priority,
                TestSuites = suites
            };
        }

        private static TestSuiteResponse MapToResponse(TestSuite suite)
        {
            return new TestSuiteResponse
            {
                Id = suite.Id,
                ProjectId = suite.ProjectId,
                Name = suite.Name,
                Description = suite.Description,
                FolderId = suite.FolderId,
                SourceTemplateId = suite.SourceTemplateId,
                CreatedAt = suite.CreatedAt,
                CreatedBy = suite.CreatedBy,
                UpdatedAt = suite.UpdatedAt,
                UpdatedBy = suite.UpdatedBy
            };
        }
    }
}