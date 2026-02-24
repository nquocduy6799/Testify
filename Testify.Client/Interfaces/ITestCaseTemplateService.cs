using Testify.Shared.DTOs.TestCases;

namespace Testify.Client.Interfaces
{
    public interface ITestCaseTemplateService
    {
        Task<TestCaseTemplateResponse> CreateTestCaseTemplateAsync(int suiteId, CreateTestCaseTemplateRequest request);
        Task<TestCaseTemplateResponse> UpdateTestCaseTemplateAsync(int id, UpdateTestCaseTemplateRequest request);
        Task<bool> DeleteTestCaseTemplateAsync(int id);
        Task<TestCaseTemplateResponse> GetTestCaseTemplateByIdAsync(int id);
    }
}
