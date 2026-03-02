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
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MilestoneResponse>>($"api/Milestones/project/{projectId}") ?? [];
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[MilestoneService] Error fetching milestones for project {projectId}: {ex.Message}");
                throw new InvalidOperationException("Failed to load milestones.", ex);
            }
        }

        public async Task<MilestoneResponse> GetMilestoneAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MilestoneResponse>($"api/Milestones/{id}")
                    ?? throw new InvalidOperationException("Milestone not found.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[MilestoneService] Error fetching milestone {id}: {ex.Message}");
                throw new InvalidOperationException("Failed to load milestone.", ex);
            }
        }

        public async Task CreateMilestoneAsync(CreateMilestoneRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Milestones", request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[MilestoneService] Error creating milestone: {ex.Message}");
                throw new InvalidOperationException("Failed to create milestone.", ex);
            }
        }

        public async Task UpdateMilestoneAsync(UpdateMilestoneRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Milestones/{request.Id}", request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[MilestoneService] Error updating milestone {request.Id}: {ex.Message}");
                throw new InvalidOperationException("Failed to update milestone.", ex);
            }
        }

        public async Task DeleteMilestoneAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Milestones/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[MilestoneService] Error deleting milestone {id}: {ex.Message}");
                throw new InvalidOperationException("Failed to delete milestone.", ex);
            }
        }

        public async Task<MilestoneResponse> GetActiveMilestoneByProjectIdAsync(int projectId)
        {
            var milestones = await GetMilestonesByProjectAsync(projectId);
            var activeMilestone = milestones.FirstOrDefault(m => m.Status == MilestoneEnum.MilestoneStatus.Active);
            return activeMilestone ?? throw new InvalidOperationException("Active milestone not found.");
        }
    }
}
