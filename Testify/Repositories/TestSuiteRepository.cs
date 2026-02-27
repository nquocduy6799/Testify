using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Repositories
{
    public class TestSuiteRepository : ITestSuiteRepository
    {
        private readonly ApplicationDbContext _context;

        public TestSuiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestSuiteResponse>> GetTestSuitesByProjectIdAsync(int projectId)
        {
            return await _context.TestSuites
                .Where(s => !s.IsDeleted && s.ProjectId == projectId)
                .Include(s => s.TestCases.Where(c => !c.IsDeleted))
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => MapToResponse(s))
                .ToListAsync();
        }

        public async Task<TestSuiteResponse?> GetTestSuiteByIdAsync(int id)
        {
            var suite = await _context.TestSuites
                .Where(s => !s.IsDeleted && s.Id == id)
                .Include(s => s.TestCases.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.TestSteps)
                .FirstOrDefaultAsync();

            return suite is null ? null : MapToResponse(suite);
        }

        public async Task<TestSuiteResponse> CreateTestSuiteAsync(CreateTestSuiteRequest request, string userName)
        {
            var suite = new TestSuite
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                FolderId = request.FolderId,
                SourceTemplateId = request.SourceTemplateId
            };
            suite.MarkAsCreated(userName);

            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();

            return MapToResponse(suite);
        }

        public async Task<TestSuiteResponse?> CreateTestSuiteFromTemplateAsync(CreateTestSuiteRequest request, string userName)
        {
            if (!request.SourceTemplateId.HasValue)
                return null;

            var template = await _context.TestSuiteTemplates
                .Where(t => !t.IsDeleted && t.Id == request.SourceTemplateId.Value)
                .Include(t => t.TestCaseTemplates)
                    .ThenInclude(c => c.TestStepTemplates)
                .FirstOrDefaultAsync();

            if (template is null)
                return null;

            var suite = new TestSuite
            {
                ProjectId = request.ProjectId,
                Name = string.IsNullOrWhiteSpace(request.Name) ? template.Name : request.Name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? template.Description : request.Description,
                FolderId = request.FolderId,
                SourceTemplateId = template.Id
            };
            suite.MarkAsCreated(userName);

            // Clone cases & steps from template
            foreach (var caseTemplate in template.TestCaseTemplates)
            {
                var testCase = new TestCase
                {
                    Title = caseTemplate.Title,
                    Preconditions = caseTemplate.Preconditions,
                    Postconditions = caseTemplate.Postconditions,
                    Priority = caseTemplate.Priority
                };
                testCase.MarkAsCreated(userName);

                foreach (var stepTemplate in caseTemplate.TestStepTemplates)
                {
                    testCase.TestSteps.Add(new TestStep
                    {
                        StepNumber = stepTemplate.StepNumber,
                        Action = stepTemplate.Action,
                        TestData = stepTemplate.TestData,
                        ExpectedResult = stepTemplate.ExpectedResult
                    });
                }

                suite.TestCases.Add(testCase);
            }

            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetTestSuiteByIdAsync(suite.Id);
        }

        public async Task<bool> UpdateTestSuiteAsync(int id, UpdateTestSuiteRequest request, string userName)
        {
            var suite = await _context.TestSuites
                .Where(s => !s.IsDeleted && s.Id == id)
                .FirstOrDefaultAsync();

            if (suite is null) return false;

            suite.Name = request.Name;
            suite.Description = request.Description;
            suite.FolderId = request.FolderId;
            suite.MarkAsUpdated(userName);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestSuiteAsync(int id, string userName)
        {
            var suite = await _context.TestSuites
                .Where(s => !s.IsDeleted && s.Id == id)
                .FirstOrDefaultAsync();

            if (suite is null) return false;

            suite.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TestSuiteResponse MapToResponse(TestSuite s)
        {
            return new TestSuiteResponse
            {
                Id = s.Id,
                ProjectId = s.ProjectId,
                Name = s.Name,
                Description = s.Description,
                FolderId = s.FolderId,
                SourceTemplateId = s.SourceTemplateId,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
                TestCases = s.TestCases
                    .Where(c => !c.IsDeleted)
                    .Select(c => new TestCaseResponse
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
                    }).ToList()
            };
        }
    }
}
