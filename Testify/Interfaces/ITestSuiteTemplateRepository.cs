using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TestTemplates;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ITestSuiteTemplateRepository
    {
        Task<IEnumerable<TestSuiteTemplateResponse>> GetAllTestSuiteTemplatesAsync();
        Task<IEnumerable<TestSuiteTemplateResponse>> GetCloneableTemplatesAsync(string userId);
        Task<TestSuiteTemplateResponse?> GetTestSuiteTemplateByIdAsync(int id);
        Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request, string userName, string userId);
        Task<bool> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request, string userName);
        Task<bool> DeleteTestSuiteTemplateAsync(int id, string userName);
    }
}

