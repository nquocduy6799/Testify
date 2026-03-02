using Testify.Shared.DTOs.Bugs;
using Testify.Shared.DTOs.Tags;

namespace Testify.Client.Interfaces
{
    public interface ITagService
    {
        Task<List<TagResponse>> GetAllTagsAsync();
        Task<TagResponse?> GetTagByIdAsync(int id);
        Task<TagResponse> CreateTagAsync(CreateTagRequest request);
        Task DeleteTagAsync(int id);
    }
}
