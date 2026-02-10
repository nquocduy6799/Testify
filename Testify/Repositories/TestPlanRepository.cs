using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class TestPlanRepository : ITestPlanRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITestPlanSuiteRepository _testPlanSuiteRepository;

        public TestPlanRepository(ApplicationDbContext context, ITestPlanSuiteRepository testPlanSuiteRepository)
        {
            _context = context;
            _testPlanSuiteRepository = testPlanSuiteRepository;
        }

        public async Task<IEnumerable<TestPlanResponse>> GetAllTestPlansAsync(int projectId)
        {
            return await _context.TestPlans
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .Include(p => p.TestPlanSuites)
                    .ThenInclude(tps => tps.TestSuite)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new TestPlanResponse
                {
                    Id = p.Id,
                    ProjectId = p.ProjectId,
                    TaskId = p.TaskId,
                    MilestoneId = p.MilestoneId,
                    Scope = p.Scope,
                    Name = p.Name,
                    Status = p.Status,
                    Outcome = p.Outcome,
                    StartedAt = p.StartedAt,
                    CompletedAt = p.CompletedAt,
                    Priority = p.Priority,
                    TestSuites = p.TestPlanSuites
                        .Where(tps => !tps.TestSuite.IsDeleted)
                        .Select(tps => new TestSuiteResponse
                        {
                            Id = tps.TestSuite.Id,
                            ProjectId = tps.TestSuite.ProjectId,
                            Name = tps.TestSuite.Name,
                            Description = tps.TestSuite.Description,
                            FolderId = tps.TestSuite.FolderId,
                            SourceTemplateId = tps.TestSuite.SourceTemplateId,
                            CreatedAt = tps.TestSuite.CreatedAt,
                            CreatedBy = tps.TestSuite.CreatedBy,
                            UpdatedAt = tps.TestSuite.UpdatedAt,
                            UpdatedBy = tps.TestSuite.UpdatedBy
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<TestPlanResponse?> GetTestPlanByIdAsync(int id)
        {
            return await _context.TestPlans
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new TestPlanResponse
                {
                    Id = p.Id,
                    ProjectId = p.ProjectId,
                    TaskId = p.TaskId,
                    MilestoneId = p.MilestoneId,
                    Scope = p.Scope,
                    Name = p.Name,
                    Status = p.Status,
                    Outcome = p.Outcome,
                    StartedAt = p.StartedAt,
                    CompletedAt = p.CompletedAt,
                    Priority = p.Priority
                })
                .FirstOrDefaultAsync();
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
                Status = request.Status,
                Priority = request.Priority
            };

            testPlan.MarkAsCreated(userName);

            // Add test suites if provided
            if (request.TestSuiteIds != null && request.TestSuiteIds.Any())
            {
                await _testPlanSuiteRepository.CreateTestPlanSuiteAsync(testPlan.Id, request.TestSuiteIds);
            }

            _context.TestPlans.Add(testPlan);
            await _context.SaveChangesAsync();

            return MapToResponse(testPlan);
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
            testPlan.Status = request.Status;
            testPlan.Priority = request.Priority;
            testPlan.MarkAsUpdated(userName);

            // Update test suites
            if (request.TestSuiteIds != null)
            {
                await _testPlanSuiteRepository.UpdateTestPlanSuiteAsync(testPlan.Id, request.TestSuiteIds);
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TestPlanExistsAsync(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteTestPlanAsync(int id, string userName)
        {
            var testPlan = await _context.TestPlans.FindAsync(id);

            if (testPlan == null || testPlan.IsDeleted)
                return false;

            testPlan.MarkAsDeleted(userName);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TestPlanExistsAsync(id))
                    return false;

                throw;
            }
        }

        private async Task<bool> TestPlanExistsAsync(int id)
        {
            return await _context.TestPlans.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        private static TestPlanResponse MapToResponse(TestPlan testPlan)
        {
            return new TestPlanResponse
            {
                Id = testPlan.Id,
                ProjectId = testPlan.ProjectId,
                TaskId = testPlan.TaskId,
                MilestoneId = testPlan.MilestoneId,
                Scope = testPlan.Scope,
                Name = testPlan.Name,
                Status = testPlan.Status,
                Outcome = testPlan.Outcome,
                StartedAt = testPlan.StartedAt,
                CompletedAt = testPlan.CompletedAt,
                Priority = testPlan.Priority,
                TestSuites = testPlan.TestPlanSuites.Select(tps => new TestSuiteResponse
                {
                    Id = tps.TestSuite.Id,
                    ProjectId = tps.TestSuite.ProjectId,
                    Name = tps.TestSuite.Name,
                    Description = tps.TestSuite.Description,
                    FolderId = tps.TestSuite.FolderId,
                    SourceTemplateId = tps.TestSuite.SourceTemplateId,
                    CreatedAt = tps.TestSuite.CreatedAt,
                    CreatedBy = tps.TestSuite.CreatedBy,
                    UpdatedAt = tps.TestSuite.UpdatedAt,
                    UpdatedBy = tps.TestSuite.UpdatedBy
                }).ToList()
            };
        }
    }
}