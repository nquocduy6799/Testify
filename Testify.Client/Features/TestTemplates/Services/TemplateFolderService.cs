using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TemplateFolders;

namespace Testify.Client.Features.TestTemplates.Services
{
    public class TemplateFolderService : ITemplateFolderService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/templatefolders";

        public TemplateFolderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TemplateFolderResponse>> GetTemplateFoldersAsync()
        {
            try
            {
                var folders = await _httpClient.GetFromJsonAsync<List<TemplateFolderResponse>>(ApiEndpoint);
                return folders ?? new List<TemplateFolderResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TemplateFolderResponse>();
            }
        }

        public async Task<TemplateFolderResponse> GetTemplateFolderByIdAsync(int id)
        {
            try
            {
                var folder = await _httpClient.GetFromJsonAsync<TemplateFolderResponse>($"{ApiEndpoint}/{id}");
                return folder ?? throw new InvalidOperationException($"Template folder with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve template folder with ID {id}.", ex);
            }
        }

        public async Task<TemplateFolderResponse> CreateTemplateFolderAsync(CreateTemplateFolderRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<TemplateFolderResponse>();
                return created ?? throw new InvalidOperationException("Failed to create template folder.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create template folder.", ex);
            }
        }

        public async Task<TemplateFolderResponse> UpdateTemplateFolderAsync(int id, UpdateTemplateFolderRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();

                // Since PUT returns NoContent (204), fetch the updated folder
                return await GetTemplateFolderByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update template folder with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTemplateFolderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}