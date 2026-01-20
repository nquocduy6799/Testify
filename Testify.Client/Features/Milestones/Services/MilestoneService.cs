using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Milestones;

namespace Testify.Client.Features.Milestones.Services
{
    public class MilestoneService(HttpClient httpClient) : IMilestoneService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<MilestoneDTO>> GetMilestonesByProjectAsync(int projectId)
        {
            return await _httpClient.GetFromJsonAsync<List<MilestoneDTO>>($"api/Milestones/project/{projectId}") ?? [];
        }

        public async Task<MilestoneDTO> GetMilestoneAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MilestoneDTO>($"api/Milestones/{id}")
                ?? throw new Exception("Milestone not found");
        }

        public async Task CreateMilestoneAsync(CreateMilestoneDTO request)
        {
            await _httpClient.PostAsJsonAsync("api/Milestones", request);
        }

        public async Task UpdateMilestoneAsync(UpdateMilestoneDTO request)
        {
            await _httpClient.PutAsJsonAsync($"api/Milestones/{request.Id}", request);
        }

        public async Task DeleteMilestoneAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/Milestones/{id}");
        }
    }
}
