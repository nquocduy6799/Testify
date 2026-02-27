using Testify.Entities;

namespace Testify.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<TemplateCategory>> GetAllCategoriesAsync();
        Task<TemplateCategory?> GetCategoryByIdAsync(int id);
        Task<TemplateCategory> CreateCategoryAsync(TemplateCategory category);
        Task<TemplateCategory> UpdateCategoryAsync(TemplateCategory category);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
