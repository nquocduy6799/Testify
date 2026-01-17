using Testify.Shared.DTOs.Projects;

namespace Testify.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<ProjectResponse>> GetUserProjectsAsync(string userId);
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string userId, string userName);
        Task<bool> UpdateProjectAsync(int id, UpdateProjectRequest request, string userName);
        Task<bool> DeleteProjectAsync(int id, string userName);
    }
}
