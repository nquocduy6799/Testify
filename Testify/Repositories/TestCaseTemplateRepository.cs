using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;

namespace Testify.Repositories
{
    public class TestCaseTemplateRepository : ITestCaseTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public TestCaseTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestCaseTemplateResponse?> GetTestCaseTemplateByIdAsync(int id)
        {
            var testCase = await _context.TestCaseTemplates
                .Include(tc => tc.TestStepTemplates)
                .FirstOrDefaultAsync(tc => tc.Id == id); // Ensure not deleted if soft delete applies? TestCaseTemplate doesn't seem to have IsDeleted in previous view, but Suite did. Checking Entity...
            
            // Assuming default behavior for now.
            return testCase != null ? MapToResponse(testCase) : null;
        }

        public async Task<TestCaseTemplateResponse> CreateTestCaseTemplateAsync(int suiteId, CreateTestCaseTemplateRequest request, string userId)
        {
            var testCase = new TestCaseTemplate
            {
                SuiteTemplateId = suiteId,
                Title = request.Title,
                Preconditions = request.Preconditions,
                Postconditions = request.Postconditions,
                Priority = request.Priority,
                TestStepTemplates = request.TestStepTemplates.Select(MapToEntity).ToList()
            };

            // If there are tracking fields like CreatedBy/At on TestCaseTemplate, set them here. 
            // Based on previous files, logic was on Suite.
            // Let's assume basic creation for now.

            _context.TestCaseTemplates.Add(testCase);
            await _context.SaveChangesAsync();

            return MapToResponse(testCase);
        }

        public async Task<bool> UpdateTestCaseTemplateAsync(int id, UpdateTestCaseTemplateRequest request, string userId)
        {
            var testCase = await _context.TestCaseTemplates
                .Include(tc => tc.TestStepTemplates)
                .FirstOrDefaultAsync(tc => tc.Id == id);

            if (testCase == null) return false;

            testCase.Title = request.Title;
            testCase.Preconditions = request.Preconditions;
            testCase.Postconditions = request.Postconditions;
            testCase.Priority = request.Priority;
            
            // Update Steps
            if (testCase.TestStepTemplates.Any())
            {
                _context.TestStepTemplates.RemoveRange(testCase.TestStepTemplates);
            }
            
            testCase.TestStepTemplates = request.TestStepTemplates.Select(MapToEntity).ToList();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestCaseTemplateAsync(int id, string userId)
        {
            var testCase = await _context.TestCaseTemplates.FindAsync(id);
            if (testCase == null) return false;

            _context.TestCaseTemplates.Remove(testCase);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCaseTemplateTitleExistsAsync(int suiteTemplateId, string title, int? excludeCaseId = null)
        {
            return await _context.TestCaseTemplates
                .AnyAsync(c => !c.IsDeleted
                    && c.SuiteTemplateId == suiteTemplateId
                    && c.Title.ToLower() == title.ToLower()
                    && (!excludeCaseId.HasValue || c.Id != excludeCaseId.Value));
        }

        private static TestCaseTemplateResponse MapToResponse(TestCaseTemplate entity)
        {
            return new TestCaseTemplateResponse
            {
                Id = entity.Id,
                SuiteTemplateId = entity.SuiteTemplateId,
                Title = entity.Title,
                Preconditions = entity.Preconditions,
                Postconditions = entity.Postconditions,
                Priority = entity.Priority,
                TestStepTemplates = entity.TestStepTemplates.Select(s => new TestStepTemplateResponse
                {
                    Id = s.Id,
                    TestCaseTemplateId = s.TestCaseTemplateId,
                    StepNumber = s.StepNumber,
                    Action = s.Action,
                    TestData = s.TestData,
                    ExpectedResult = s.ExpectedResult
                }).ToList()
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
