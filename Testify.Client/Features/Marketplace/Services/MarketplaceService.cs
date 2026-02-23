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

        // Gọi API lấy danh sách Template
        public async Task<List<TemplateDto>> GetTemplatesAsync()
        {
            return await _http.GetFromJsonAsync<List<TemplateDto>>("api/marketplace") ?? new List<TemplateDto>();
        }

        // Gọi API Clone Template
        public async Task<bool> CloneTemplateAsync(int templateId, int targetProjectId)
        {
            var request = new CloneTemplateRequest 
            { 
                TemplateId = templateId, 
                TargetProjectId = targetProjectId 
            };
            
            var response = await _http.PostAsJsonAsync("api/marketplace/clone", request);
            return response.IsSuccessStatusCode;
        }

        // Gọi API lấy danh sách Categories
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>("api/marketplace/categories") ?? new List<CategoryDto>();
        }

        // Gọi API tạo Category mới
        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto category)
        {
            var response = await _http.PostAsJsonAsync("api/marketplace/categories", category);
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? category;
        }

        // Gọi API cập nhật Category
        public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto category)
        {
            var response = await _http.PutAsJsonAsync($"api/marketplace/categories/{category.Id}", category);
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? category;
        }

        // Gọi API xóa Category
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var response = await _http.DeleteAsync($"api/marketplace/categories/{categoryId}");
            return response.IsSuccessStatusCode;
        }

        // Template CRUD (Admin)
        public async Task<List<TemplateDto>> GetAllTemplatesAsync()
        {
            return await _http.GetFromJsonAsync<List<TemplateDto>>("api/marketplace/templates/all") ?? new List<TemplateDto>();
        }

        public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/marketplace/templates", dto);
            return await response.Content.ReadFromJsonAsync<TemplateDto>() ?? new TemplateDto();
        }

        public async Task<TemplateDto> UpdateTemplateAsync(int id, CreateTemplateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/marketplace/templates/{id}", dto);
            return await response.Content.ReadFromJsonAsync<TemplateDto>() ?? new TemplateDto();
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var response = await _http.DeleteAsync($"api/marketplace/templates/{templateId}");
            return response.IsSuccessStatusCode;
        }
    }
}
