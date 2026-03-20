using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
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
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            return templates.Select(MapToResponse).ToList();
        }

        public async Task<IEnumerable<TestSuiteTemplateResponse>> GetCloneableTemplatesAsync(string userId)
        {
            var templates = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                    .ThenInclude(tc => tc.TestStepTemplates)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => !t.IsDeleted && (t.IsPublic || t.UserId == userId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return templates.Select(MapToResponse).ToList();
        }

        public async Task<TestSuiteTemplateResponse?> GetTestSuiteTemplateByIdAsync(int id)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                    .ThenInclude(tc => tc.TestStepTemplates)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            return template != null ? MapToResponse(template) : null;
        }

        public async Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request, string userName, string userId)
        {
            var template = new TestSuiteTemplate
            {
                Name = request.Name,
                Description = request.Description,
                FolderId = request.FolderId,
                CategoryId = request.CategoryId,
                IsPublic = request.IsPublic,
                UserId = userId,
                TestCaseTemplates = request.TestCaseTemplates
                    .Select(MapToEntity)
                    .ToList()
            };

            template.MarkAsCreated(userName);

            _context.TestSuiteTemplates.Add(template);
            await _context.SaveChangesAsync();

            foreach (var tagId in request.TagIds)
            {
                _context.TestSuiteTemplateTags.Add(new TestSuiteTemplateTag
                {
                    TemplateId = template.Id,
                    TagId = tagId
                });
            }

            if (request.TagIds.Count > 0)
                await _context.SaveChangesAsync();

            var savedTemplate = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                    .ThenInclude(tc => tc.TestStepTemplates)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .FirstAsync(t => t.Id == template.Id);

            return MapToResponse(savedTemplate);
        }

        public async Task<bool> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request, string userName)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                .ThenInclude(tc => tc.TestStepTemplates)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (template == null)
                return false;

            template.Name = request.Name;
            template.Description = request.Description;
            template.FolderId = request.FolderId;
            template.CategoryId = request.CategoryId;
            template.IsPublic = request.IsPublic;
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

            _context.TestSuiteTemplateTags.RemoveRange(template.Tags);
            foreach (var tagId in request.TagIds)
            {
                _context.TestSuiteTemplateTags.Add(new TestSuiteTemplateTag
                {
                    TemplateId = template.Id,
                    TagId = tagId
                });
            }

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

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var template = await _context.TestSuiteTemplates.FindAsync(id);
            if (template == null || template.IsDeleted) return false;
            template.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementCloneCountAsync(int id)
        {
            var template = await _context.TestSuiteTemplates.FindAsync(id);
            if (template == null || template.IsDeleted) return false;
            template.CloneCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int deleted, int failed)> BulkDeleteAsync(List<int> ids, string userName)
        {
            int deleted = 0, failed = 0;
            foreach (var id in ids)
            {
                var ok = await DeleteTestSuiteTemplateAsync(id, userName);
                if (ok) deleted++; else failed++;
            }
            return (deleted, failed);
        }

        public async Task<(int moved, int failed)> BulkMoveAsync(List<int> ids, int? targetFolderId, string userName)
        {
            int moved = 0, failed = 0;
            foreach (var id in ids)
            {
                var template = await _context.TestSuiteTemplates.FindAsync(id);
                if (template == null || template.IsDeleted) { failed++; continue; }
                template.FolderId = targetFolderId;
                template.MarkAsUpdated(userName);
                moved++;
            }
            await _context.SaveChangesAsync();
            return (moved, failed);
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
                CategoryId = template.CategoryId,
                Name = template.Name,
                Description = template.Description,
                IsPublic = template.IsPublic,
                CloneCount = template.CloneCount,
                ViewCount = template.ViewCount,
                TotalStarred = template.TotalStarred,
                ShareCode = template.ShareCode,
                CreatedAt = template.CreatedAt,
                CreatedBy = template.CreatedBy,
                UpdatedAt = template.UpdatedAt,
                UpdatedBy = template.UpdatedBy,
                Category = template.Category != null ? new Testify.Shared.DTOs.Categories.CategoryResponse
                {
                    Id = template.Category.Id,
                    Name = template.Category.Name ?? string.Empty
                } : null,
                Tags = template.Tags.Select(tt => new Testify.Shared.DTOs.Tags.TagResponse
                {
                    Id = tt.Tag.Id,
                    TagName = tt.Tag.TagName ?? string.Empty
                }).ToList(),
                TestCaseTemplates = template.TestCaseTemplates.Select(tc => new TestCaseTemplateResponse
                {
                    Id = tc.Id,
                    SuiteTemplateId = tc.SuiteTemplateId,
                    Title = tc.Title,
                    Preconditions = tc.Preconditions,
                    Postconditions = tc.Postconditions,
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
                Preconditions = request.Preconditions,
                Postconditions = request.Postconditions,
                Priority = request.Priority,
                TestStepTemplates = request.TestStepTemplates.Select(MapToEntity).ToList()
            };
        }

        private static TestCaseTemplate MapToEntity(UpdateTestCaseTemplateRequest request)
        {
            return new TestCaseTemplate
            {
                Title = request.Title,
                Preconditions = request.Preconditions,
                Postconditions = request.Postconditions,
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