using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TestTemplates;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ITestSuiteTemplateRepository
    {
        Task<IEnumerable<TestSuiteTemplateResponse>> GetAllTestSuiteTemplatesAsync();
        Task<TestSuiteTemplateResponse?> GetTestSuiteTemplateByIdAsync(int id);
        Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request, string userName);
        Task<bool> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request, string userName);
        Task<bool> DeleteTestSuiteTemplateAsync(int id, string userName);
    }
}

