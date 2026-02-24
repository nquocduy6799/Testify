using Testify.Shared.DTOs.TestSuites;

namespace Testify.Client.Interfaces
{
    public interface ITestCaseService
    {
        Task<TestCaseResponse> GetTestCaseByIdAsync(int id);
        Task<TestCaseResponse> CreateTestCaseAsync(int suiteId, CreateTestCaseRequest request);
        Task<bool> UpdateTestCaseAsync(int id, UpdateTestCaseRequest request);
        Task<bool> DeleteTestCaseAsync(int id);
    }
}
