using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface ITestPlanService
    {
        Task<List<TestPlanResponse>> GetAllTestPlansAsync(int projectId);
        Task<TestPlanResponse?> GetTestPlanByIdAsync(int id);
        Task<TestPlanResponse> CreateTestPlanAsync(CreateTestPlanRequest request);
        Task<bool> UpdateTestPlanAsync(int id, UpdateTestPlanRequest request);
        Task<bool> DeleteTestPlanAsync(int id);

        //Operations for Test Suites within Project
        Task<IEnumerable<TestSuiteResponse>> GetAllTestSuitesByProjectIdAsync(int projectId);

    }
}
