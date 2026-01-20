using Testify.Shared.DTOs.Milestones;

namespace Testify.Client.Interfaces
{
    public interface IMilestoneService
    {
        Task<List<MilestoneResponse>> GetMilestonesByProjectAsync(int projectId);
        Task<MilestoneResponse> GetMilestoneAsync(int id);
        Task CreateMilestoneAsync(CreateMilestoneRequest request);
        Task UpdateMilestoneAsync(UpdateMilestoneRequest request);
        Task DeleteMilestoneAsync(int id);
    }
}
