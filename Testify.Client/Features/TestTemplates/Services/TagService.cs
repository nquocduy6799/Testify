using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Tags;

namespace Testify.Client.Features.Account.Services
{
    public class TagService : ITagService
    {
        private readonly HttpClient _httpClient;

        public TagService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TagResponse>> GetAllTagsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<TagResponse>>("api/Tags");
            return response ?? new List<TagResponse>();
        }

        public async Task<TagResponse?> GetTagByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TagResponse>($"api/Tags/{id}");
        }

        public async Task<TagResponse> CreateTagAsync(CreateTagRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Tags", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TagResponse>() ?? throw new Exception("Failed to create tag");
        }

        public async Task DeleteTagAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Tags/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
