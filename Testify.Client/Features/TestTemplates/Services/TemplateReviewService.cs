using System.Net.Http.Json;
using Testify.Client.Interfaces;

namespace Testify.Client.Features.Account.Services
{
    public class TemplateReviewService : ITemplateReviewService
    {
        private readonly HttpClient _httpClient;

        public TemplateReviewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<int>> GetStarredTemplateIdsAsync()
        {
            var response = await _httpClient.GetAsync("api/TemplateReviews/starred");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<int>>() ?? new List<int>();
        }

        public async Task StarTemplateAsync(int templateId)
        {
            var response = await _httpClient.PostAsync($"api/TemplateReviews/{templateId}/star", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnstarTemplateAsync(int templateId)
        {
            var response = await _httpClient.DeleteAsync($"api/TemplateReviews/{templateId}/star");
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetStarCountAsync(int templateId)
        {
            var response = await _httpClient.GetAsync($"api/TemplateReviews/{templateId}/stars/count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}
