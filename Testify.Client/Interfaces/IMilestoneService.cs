using Testify.Shared.DTOs.Milestones;

namespace Testify.Client.Interfaces
{
    public interface IMilestoneService
    {
        Task<List<MilestoneDTO>> GetMilestonesByProjectAsync(int projectId);
        Task<MilestoneDTO> GetMilestoneAsync(int id);
        Task CreateMilestoneAsync(CreateMilestoneDTO request);
        Task UpdateMilestoneAsync(UpdateMilestoneDTO request);
        Task DeleteMilestoneAsync(int id);
    }
}
