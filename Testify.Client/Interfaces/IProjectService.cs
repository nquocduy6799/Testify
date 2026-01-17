using Testify.Shared.DTOs.Projects;

namespace Testify.Client.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectResponse>> GetProjectsAsync();
        Task<ProjectResponse> GetProjectByIdAsync(int id);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
        Task<ProjectResponse> UpdateProjectAsync(int id, UpdateProjectRequest request);
        Task<bool> DeleteProjectAsync(int id);
    }
}

