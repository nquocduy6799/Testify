using Testify.Entities;
using Testify.Shared.DTOs.TestCases;


namespace Testify.Interfaces
{
    public interface ITestPlanSuiteRepository
    {
        // Get operations
        Task<IEnumerable<TestSuiteResponse>> GetTestSuitesByTestPlanIdAsync(int testPlanId);
        Task<bool> IsTestSuiteLinkedToTestPlanAsync(int testPlanId, int testSuiteId);

        // Create operations
        Task<bool> CreateTestPlanSuiteAsync(int testPlanId, List<int> testSuiteIds);
        Task<bool> AddTestSuiteToTestPlanAsync(int testPlanId, int testSuiteId);

        // Update operations
        Task<bool> UpdateTestPlanSuiteAsync(int testPlanId, List<int> testSuiteIds);

        // Delete operations
        Task<bool> RemoveTestSuiteFromTestPlanAsync(int testPlanId, int testSuiteId);
        Task<bool> RemoveAllTestSuitesFromTestPlanAsync(int testPlanId);

        // Operation for test suites
        Task<IEnumerable<TestSuiteResponse>> GetAllTestSuitesByProjectIdAsync(int projectId);
    }
}