using Testify.Shared.DTOs.Marketplace;

namespace Testify.Client.Interfaces
{
    public interface IMarketplaceService
    {
        Task<List<TemplateDto>> GetTemplatesAsync();
        Task<bool> CloneTemplateAsync(int templateId, int targetProjectId);
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CategoryDto category);
        Task<CategoryDto> UpdateCategoryAsync(CategoryDto category);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Template CRUD (Admin)
        Task<List<TemplateDto>> GetAllTemplatesAsync();
        Task<TemplateDto> CreateTemplateAsync(CreateTemplateDto dto);
        Task<TemplateDto> UpdateTemplateAsync(int id, CreateTemplateDto dto);
        Task<bool> DeleteTemplateAsync(int templateId);
    }
}
