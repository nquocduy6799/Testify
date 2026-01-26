using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Client.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectResponse>> GetProjectsAsync();
        Task<ProjectResponse> GetProjectByIdAsync(int id);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
        Task<ProjectResponse> UpdateProjectAsync(int id, UpdateProjectRequest request);
        Task<bool> DeleteProjectAsync(int id);
        Task<ProjectRole?> GetMyRoleInProjectAsync(int projectId);
        Task<List<TeamMemberResponse>> GetMembersAsync(int projectId);
        Task<bool> RemoveMemberAsync(int projectId, int memberId);
    }
}

