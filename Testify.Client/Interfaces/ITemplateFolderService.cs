using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TemplateFolders;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface ITemplateFolderService
    {
        Task<List<TemplateFolderResponse>> GetTemplateFoldersAsync();
        Task<TemplateFolderResponse> GetTemplateFolderByIdAsync(int id);
        Task<TemplateFolderResponse> CreateTemplateFolderAsync(CreateTemplateFolderRequest request);
        Task<TemplateFolderResponse> UpdateTemplateFolderAsync(int id, UpdateTemplateFolderRequest request);
        Task<bool> DeleteTemplateFolderAsync(int id);

    }
}
