using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Categories;

namespace Testify.Client.Features.Account.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;

        public CategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<CategoryResponse>>("api/Categories");
            return response ?? new List<CategoryResponse>();
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<CategoryResponse>($"api/Categories/{id}");
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Categories", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryResponse>() ?? throw new Exception("Failed to create category");
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Categories/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
