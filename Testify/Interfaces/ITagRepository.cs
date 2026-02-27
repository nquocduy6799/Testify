using Testify.Entities;

namespace Testify.Interfaces
{
    public interface ITagRepository
    {
        Task<List<TemplateTag>> GetAllTagsAsync();
        Task<TemplateTag?> GetTagByIdAsync(int id);
        Task<TemplateTag?> GetTagByNameAsync(string name);
        Task<TemplateTag> CreateTagAsync(TemplateTag tag);
        Task<bool> DeleteTagAsync(int id);
    }
}
