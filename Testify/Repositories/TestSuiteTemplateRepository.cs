using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestTemplates;

namespace Testify.Repositories
{
    public class TestSuiteTemplateRepository : ITestSuiteTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public TestSuiteTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestSuiteTemplateResponse>> GetAllTestSuiteTemplatesAsync()
        {
            var templates = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                .ThenInclude(tc => tc.TestStepTemplates)
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            return templates.Select(MapToResponse).ToList();
        }

        public async Task<TestSuiteTemplateResponse?> GetTestSuiteTemplateByIdAsync(int id)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                .ThenInclude(tc => tc.TestStepTemplates)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            return template != null ? MapToResponse(template) : null;
        }

        public async Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request, string userName)
        {
            var template = new TestSuiteTemplate
            {
                Name = request.Name,
                Description = request.Description,
                FolderId = request.FolderId,
                UserId = userName,
                TestCaseTemplates = request.TestCaseTemplates
                    .Select(MapToEntity)
                    .ToList()
            };

            template.MarkAsCreated(userName);

            _context.TestSuiteTemplates.Add(template);
            await _context.SaveChangesAsync();

            return MapToResponse(template);
        }

        public async Task<bool> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request, string userName)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                .ThenInclude(tc => tc.TestStepTemplates)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (template == null)
                return false;

            template.Name = request.Name;
            template.Description = request.Description;
            template.FolderId = request.FolderId;
            template.MarkAsUpdated(userName);

            var existingSteps = template.TestCaseTemplates
                .SelectMany(tc => tc.TestStepTemplates)
                .ToList();

            if (existingSteps.Count > 0)
                _context.TestStepTemplates.RemoveRange(existingSteps);

            if (template.TestCaseTemplates.Count > 0)
                _context.TestCaseTemplates.RemoveRange(template.TestCaseTemplates);

            template.TestCaseTemplates = request.TestCaseTemplates
                .Select(MapToEntity)
                .ToList();

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TestSuiteTemplateExistsAsync(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteTestSuiteTemplateAsync(int id, string userName)
        {
            var template = await _context.TestSuiteTemplates.FindAsync(id);

            if (template == null || template.IsDeleted)
                return false;

            template.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> TestSuiteTemplateExistsAsync(int id)
        {
            return await _context.TestSuiteTemplates.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }

        private static TestSuiteTemplateResponse MapToResponse(TestSuiteTemplate template)
        {
            return new TestSuiteTemplateResponse
            {
                Id = template.Id,
                FolderId = template.FolderId,
                Name = template.Name,
                Description = template.Description,
                CreatedAt = template.CreatedAt,
                CreatedBy = template.CreatedBy,
                UpdatedAt = template.UpdatedAt,
                UpdatedBy = template.UpdatedBy,
                TestCaseTemplates = template.TestCaseTemplates.Select(tc => new TestCaseTemplateResponse
                {
                    Id = tc.Id,
                    SuiteTemplateId = tc.SuiteTemplateId,
                    Title = tc.Title,
                    Priority = tc.Priority,
                    TestStepTemplates = tc.TestStepTemplates.Select(ts => new TestStepTemplateResponse
                    {
                        Id = ts.Id,
                        TestCaseTemplateId = ts.TestCaseTemplateId,
                        StepNumber = ts.StepNumber,
                        Action = ts.Action,
                        TestData = ts.TestData,
                        ExpectedResult = ts.ExpectedResult
                    }).ToList()
                }).ToList()
            };
        }

        private static TestCaseTemplate MapToEntity(CreateTestCaseTemplateRequest request)
        {
            return new TestCaseTemplate
            {
                Title = request.Title,
                Priority = request.Priority,
                TestStepTemplates = request.TestStepTemplates.Select(MapToEntity).ToList()
            };
        }

        private static TestCaseTemplate MapToEntity(UpdateTestCaseTemplateRequest request)
        {
            return new TestCaseTemplate
            {
                Title = request.Title,
                Priority = request.Priority,
                TestStepTemplates = request.TestStepTemplates.Select(MapToEntity).ToList()
            };
        }

        private static TestStepTemplate MapToEntity(CreateTestStepTemplateRequest request)
        {
            return new TestStepTemplate
            {
                StepNumber = request.StepNumber,
                Action = request.Action,
                TestData = request.TestData,
                ExpectedResult = request.ExpectedResult
            };
        }

        private static TestStepTemplate MapToEntity(UpdateTestStepTemplateRequest request)
        {
            return new TestStepTemplate
            {
                StepNumber = request.StepNumber,
                Action = request.Action,
                TestData = request.TestData,
                ExpectedResult = request.ExpectedResult
            };
        }
    }
}