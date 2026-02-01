using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TestTemplates;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface ITestSuiteTemplateService
    {
        Task<List<TestSuiteTemplateResponse>> GetTestSuiteTemplatesAsync();
        Task<TestSuiteTemplateResponse> GetTestSuiteTemplateByIdAsync(int id);
        Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request);
        Task<TestSuiteTemplateResponse> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request);
        Task<bool> DeleteTestSuiteTemplateAsync(int id);
    }
}

