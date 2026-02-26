using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Marketplace;

namespace Testify.Client.Features.Marketplace.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private readonly HttpClient _http;

        public MarketplaceService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TemplateDto>> GetTemplatesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<TemplateDto>>("api/marketplace");
            return result ?? new();
        }

        public async Task<bool> CloneTemplateAsync(int templateId, int targetProjectId)
        {
            var response = await _http.PostAsJsonAsync("api/marketplace/clone", new CloneTemplateRequest
            {
                TemplateId = templateId,
                TargetProjectId = targetProjectId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<CategoryDto>>("api/marketplace/categories");
            return result ?? new();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto category)
        {
            var response = await _http.PostAsJsonAsync("api/marketplace/categories", category);
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? category;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto category)
        {
            var response = await _http.PutAsJsonAsync($"api/marketplace/categories/{category.Id}", category);
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? category;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var response = await _http.DeleteAsync($"api/marketplace/categories/{categoryId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<TemplateDto>> GetAllTemplatesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<TemplateDto>>("api/marketplace/templates/all");
            return result ?? new();
        }

        public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/marketplace/templates", dto);
            return await response.Content.ReadFromJsonAsync<TemplateDto>() ?? new();
        }

        public async Task<TemplateDto> UpdateTemplateAsync(int id, CreateTemplateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/marketplace/templates/{id}", dto);
            return await response.Content.ReadFromJsonAsync<TemplateDto>() ?? new();
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var response = await _http.DeleteAsync($"api/marketplace/templates/{templateId}");
            return response.IsSuccessStatusCode;
        }
    }
}
