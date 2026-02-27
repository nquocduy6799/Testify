using Testify.Shared.DTOs.Milestones;

namespace Testify.Interfaces
{
    public interface IMilestoneRepository
    {
        Task<IEnumerable<MilestoneResponse>> GetMilestonesByProjectIdAsync(int projectId);
        Task<MilestoneResponse?> GetMilestoneByIdAsync(int id);
        Task<MilestoneResponse> CreateMilestoneAsync(CreateMilestoneRequest request, string userName);
        Task<bool> UpdateMilestoneAsync(int id, UpdateMilestoneRequest request, string userName);
        Task<bool> DeleteMilestoneAsync(int id, string userName);
        Task<bool> MilestoneExistsAsync(int id);
    }
}
