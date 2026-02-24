using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.Helpers;


namespace Testify.Repositories
{
    public class TestPlanSuiteRepository : ITestPlanSuiteRepository
    {
        private readonly ApplicationDbContext _context;

        public TestPlanSuiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestSuiteResponse>> GetTestSuitesByTestPlanIdAsync(int testPlanId)
        {
            return await _context.TestPlanSuites
                .Where(tps => tps.TestPlanId == testPlanId)
                .Include(tps => tps.TestSuite)
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
                .ToListAsync();
        }

        public async Task<IEnumerable<TestSuiteResponse>> GetAllTestSuitesByProjectIdAsync(int projectId)
        {
            return await _context.TestSuites
                .Where(ts => ts.ProjectId == projectId && !ts.IsDeleted)
                .Select(ts => new TestSuiteResponse
                {
                    Id = ts.Id,
                    ProjectId = ts.ProjectId,
                    Name = ts.Name,
                    Description = ts.Description,
                    FolderId = ts.FolderId,
                    SourceTemplateId = ts.SourceTemplateId,
                    CreatedAt = ts.CreatedAt,
                    CreatedBy = ts.CreatedBy,
                    UpdatedAt = ts.UpdatedAt,
                    UpdatedBy = ts.UpdatedBy
                })
                .ToListAsync();
        }

        public async Task<bool> IsTestSuiteLinkedToTestPlanAsync(int testPlanId, int testSuiteId)
        {
            return await _context.TestPlanSuites
                .AnyAsync(tps => tps.TestPlanId == testPlanId && tps.TestSuiteId == testSuiteId);
        }

        // Create operations
        public async Task<bool> CreateTestPlanSuiteAsync(int testPlanId, List<int> testSuiteIds)
        {
            if (testSuiteIds == null || !testSuiteIds.Any())
                return false;

            // Verify test plan exists
            var testPlanExists = await _context.TestPlans.AnyAsync(tp => tp.Id == testPlanId && !tp.IsDeleted);
            if (!testPlanExists)
                return false;

            try
            {
                var testPlanSuites = testSuiteIds
                    .Distinct()
                    .Select(suiteId => new TestPlanSuite
                    {
                        TestPlanId = testPlanId,
                        TestSuiteId = suiteId,
                        AddedAt = DateTimeHelper.GetVietnamTime()
                    })
                    .ToList();

                await _context.TestPlanSuites.AddRangeAsync(testPlanSuites);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> AddTestSuiteToTestPlanAsync(int testPlanId, int testSuiteId)
        {
            // Check if already linked
            var exists = await IsTestSuiteLinkedToTestPlanAsync(testPlanId, testSuiteId);
            if (exists)
                return false;

            // Verify test plan exists
            var testPlanExists = await _context.TestPlans.AnyAsync(tp => tp.Id == testPlanId && !tp.IsDeleted);
            if (!testPlanExists)
                return false;

            // Verify test suite exists
            var testSuiteExists = await _context.TestSuites.AnyAsync(ts => ts.Id == testSuiteId && !ts.IsDeleted);
            if (!testSuiteExists)
                return false;

            try
            {
                var testPlanSuite = new TestPlanSuite
                {
                    TestPlanId = testPlanId,
                    TestSuiteId = testSuiteId,
                    AddedAt = DateTimeHelper.GetVietnamTime()
                };

                _context.TestPlanSuites.Add(testPlanSuite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        // Update operations
        public async Task<bool> UpdateTestPlanSuiteAsync(int testPlanId, List<int> testSuiteIds)
        {
            // Verify test plan exists
            var testPlanExists = await _context.TestPlans.AnyAsync(tp => tp.Id == testPlanId && !tp.IsDeleted);
            if (!testPlanExists)
                return false;

            try
            {
                // Remove all existing associations
                var existingSuites = await _context.TestPlanSuites
                    .Where(tps => tps.TestPlanId == testPlanId)
                    .ToListAsync();

                _context.TestPlanSuites.RemoveRange(existingSuites);

                // Add new associations if provided
                if (testSuiteIds != null && testSuiteIds.Any())
                {
                    var testPlanSuites = testSuiteIds
                        .Distinct()
                        .Select(suiteId => new TestPlanSuite
                        {
                            TestPlanId = testPlanId,
                            TestSuiteId = suiteId,
                            AddedAt = DateTimeHelper.GetVietnamTime()
                        })
                        .ToList();

                    await _context.TestPlanSuites.AddRangeAsync(testPlanSuites);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        // Delete operations
        public async Task<bool> RemoveTestSuiteFromTestPlanAsync(int testPlanId, int testSuiteId)
        {
            var testPlanSuite = await _context.TestPlanSuites
                .FirstOrDefaultAsync(tps => tps.TestPlanId == testPlanId && tps.TestSuiteId == testSuiteId);

            if (testPlanSuite == null)
                return false;

            try
            {
                _context.TestPlanSuites.Remove(testPlanSuite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> RemoveAllTestSuitesFromTestPlanAsync(int testPlanId)
        {
            var testPlanSuites = await _context.TestPlanSuites
                .Where(tps => tps.TestPlanId == testPlanId)
                .ToListAsync();

            if (!testPlanSuites.Any())
                return true;

            try
            {
                _context.TestPlanSuites.RemoveRange(testPlanSuites);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}