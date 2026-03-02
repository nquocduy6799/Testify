using Testify.Shared.DTOs.TestSuites;

namespace Testify.Interfaces
{
    public interface ITestSuiteRepository
    {
        Task<IEnumerable<TestSuiteResponse>> GetTestSuitesByProjectIdAsync(int projectId);
        Task<TestSuiteResponse?> GetTestSuiteByIdAsync(int id);
        Task<bool> IsSuiteNameExistsAsync(int projectId, string name, int? excludeSuiteId = null);
        Task<string> GenerateUniqueSuiteNameAsync(int projectId, string baseName);
        Task<TestSuiteResponse> CreateTestSuiteAsync(CreateTestSuiteRequest request, string userName);
        Task<TestSuiteResponse?> CreateTestSuiteFromTemplateAsync(CreateTestSuiteRequest request, string userName);
        Task<bool> UpdateTestSuiteAsync(int id, UpdateTestSuiteRequest request, string userName);
        Task<bool> DeleteTestSuiteAsync(int id, string userName);
    }
}
