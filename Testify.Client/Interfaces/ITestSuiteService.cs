using Testify.Shared.DTOs.TestSuites;

namespace Testify.Client.Interfaces
{
    public interface ITestSuiteService
    {
        Task<List<TestSuiteResponse>> GetTestSuitesByProjectIdAsync(int projectId);
        Task<TestSuiteResponse> GetTestSuiteByIdAsync(int id);
        Task<TestSuiteResponse> CreateTestSuiteAsync(CreateTestSuiteRequest request);
        Task<TestSuiteResponse> UpdateTestSuiteAsync(int id, UpdateTestSuiteRequest request);
        Task<bool> DeleteTestSuiteAsync(int id);
    }
}
