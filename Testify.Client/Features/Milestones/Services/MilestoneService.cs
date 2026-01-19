using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.Entities;

namespace Testify.Client.Features.Milestones.Services
{
    public class MilestoneService(HttpClient httpClient) : IMilestoneService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<Milestone>> GetMilestonesByProjectAsync(int projectId)
        {
            return await _httpClient.GetFromJsonAsync<List<Milestone>>($"api/Milestones/project/{projectId}") ?? [];
        }

        public async Task<Milestone> GetMilestoneAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Milestone>($"api/Milestones/{id}")
                ?? throw new Exception("Milestone not found");
        }

        public async Task CreateMilestoneAsync(Milestone milestone)
        {
            await _httpClient.PostAsJsonAsync("api/Milestones", milestone);
        }

        public async Task UpdateMilestoneAsync(Milestone milestone)
        {
            await _httpClient.PutAsJsonAsync($"api/Milestones/{milestone.Id}", milestone);
        }

        public async Task DeleteMilestoneAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/Milestones/{id}");
        }
    }
}
