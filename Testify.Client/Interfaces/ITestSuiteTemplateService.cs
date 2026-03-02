using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TestTemplates;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface ITestSuiteTemplateService
    {
        Task<List<TestSuiteTemplateResponse>> GetTestSuiteTemplatesAsync();
        Task<List<TestSuiteTemplateResponse>> GetCloneableTemplatesAsync();
        Task<TestSuiteTemplateResponse> GetTestSuiteTemplateByIdAsync(int id);
        Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request);
        Task<TestSuiteTemplateResponse> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request);
        Task<bool> DeleteTestSuiteTemplateAsync(int id);
        Task IncrementViewCountAsync(int id);
        Task IncrementCloneCountAsync(int id);
        Task<(int deleted, int failed)> BulkDeleteTemplatesAsync(List<int> templateIds);
        Task<(int moved, int failed)> BulkMoveTemplatesAsync(List<int> templateIds, int? targetFolderId);
    }
}

