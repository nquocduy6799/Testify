using Testify.Shared.DTOs.TestSuites;

namespace Testify.Interfaces
{
    public interface ITestCaseRepository
    {
        Task<TestCaseResponse?> GetTestCaseByIdAsync(int id);
        Task<TestCaseResponse> CreateTestCaseAsync(int suiteId, CreateTestCaseRequest request, string userName);
        Task<bool> UpdateTestCaseAsync(int id, UpdateTestCaseRequest request, string userName);
        Task<bool> DeleteTestCaseAsync(int id);
        Task<bool> IsCaseTitleExistsAsync(int suiteId, string title, int? excludeCaseId = null);
    }
}
