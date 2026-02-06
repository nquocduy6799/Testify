using Testify.Shared.DTOs.TestPlans;

namespace Testify.Interfaces
{
    public interface ITestPlanRepository
    {
        Task<IEnumerable<TestPlanResponse>> GetAllTestPlansAsync(int projectId);
        Task<TestPlanResponse?> GetTestPlanByIdAsync(int id);
        Task<TestPlanResponse> CreateTestPlanAsync(CreateTestPlanRequest request, string userName, string userId);
        Task<bool> UpdateTestPlanAsync(int id, UpdateTestPlanRequest request, string userName);
        Task<bool> DeleteTestPlanAsync(int id, string userName);
    }
}
