using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TemplateFolders;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ITemplateFolderRepository
    {
        Task<TemplateFolderResponse?> GetTemplateFolderByIdAsync(int id);
        Task<IEnumerable<TemplateFolderResponse>> GetAllTemplateFoldersByUserIdAsync(string userId);
        Task<TemplateFolderResponse> CreateTemplateFolderAsync(CreateTemplateFolderRequest request, string userName, string userId);
        Task<bool> UpdateTemplateFolderAsync(int id, UpdateTemplateFolderRequest request, string userName);
        Task<bool> DeleteTemplateFolderAsync(int id, string userName);
    }
}

