using Testify.Shared.DTOs.Categories;

namespace Testify.Client.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(int id);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task DeleteCategoryAsync(int id);
    }
}
