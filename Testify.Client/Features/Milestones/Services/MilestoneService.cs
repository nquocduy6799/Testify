using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Milestones;
using Testify.Shared.Enums;

namespace Testify.Client.Features.Milestones.Services
{
    public class MilestoneService(HttpClient httpClient) : IMilestoneService
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<MilestoneResponse>> GetMilestonesByProjectAsync(int projectId)
        {
            return await _httpClient.GetFromJsonAsync<List<MilestoneResponse>>($"api/Milestones/project/{projectId}") ?? [];
        }

        public async Task<MilestoneResponse> GetMilestoneAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MilestoneResponse>($"api/Milestones/{id}")
                ?? throw new Exception("Milestone not found");
        }

        public async Task CreateMilestoneAsync(CreateMilestoneRequest request)
        {
            await _httpClient.PostAsJsonAsync("api/Milestones", request);
        }

        public async Task UpdateMilestoneAsync(UpdateMilestoneRequest request)
        {
            await _httpClient.PutAsJsonAsync($"api/Milestones/{request.Id}", request);
        }

        public async Task DeleteMilestoneAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/Milestones/{id}");
        }

        public async Task<MilestoneResponse> GetActiveMilestoneByProjectIdAsync(int projectId)
        {
            var milestones = await _httpClient.GetFromJsonAsync<List<MilestoneResponse>>($"api/Milestones/project/{projectId}") ?? [];

            var activeMilestone = milestones.FirstOrDefault(m => m.Status == MilestoneEnum.MilestoneStatus.Active);

            return activeMilestone ?? throw new Exception("Active milestone not found");
        }
    }
}
