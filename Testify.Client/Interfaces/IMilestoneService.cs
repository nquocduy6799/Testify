using Testify.Shared.Entities;

namespace Testify.Client.Interfaces
{
    public interface IMilestoneService
    {
        Task<List<Milestone>> GetMilestonesByProjectAsync(int projectId);
        Task<Milestone> GetMilestoneAsync(int id);
        Task CreateMilestoneAsync(Milestone milestone);
        Task UpdateMilestoneAsync(Milestone milestone);
        Task DeleteMilestoneAsync(int id);
    }
}
