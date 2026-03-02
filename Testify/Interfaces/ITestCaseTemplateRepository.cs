using Testify.Shared.DTOs.TestCases;

namespace Testify.Interfaces
{
    public interface ITestCaseTemplateRepository
    {
        Task<TestCaseTemplateResponse?> GetTestCaseTemplateByIdAsync(int id);
        Task<TestCaseTemplateResponse> CreateTestCaseTemplateAsync(int suiteId, CreateTestCaseTemplateRequest request, string userId);
        Task<bool> UpdateTestCaseTemplateAsync(int id, UpdateTestCaseTemplateRequest request, string userId);
        Task<bool> DeleteTestCaseTemplateAsync(int id, string userId);
        Task<bool> IsCaseTemplateTitleExistsAsync(int suiteTemplateId, string title, int? excludeCaseId = null);
    }
}
