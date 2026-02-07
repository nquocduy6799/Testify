using Microsoft.CodeAnalysis;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<ProjectResponse>> GetUserProjectsAsync(string userId);
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string userId, string userName);
        Task<bool> UpdateProjectAsync(int id, UpdateProjectRequest request, string userName);
        Task<bool> DeleteProjectAsync(int id, string userName);
        Task<ProjectRole?> GetUserRoleInProjectAsync(int projectId, string userId);
        Task<ProjectUserContext?> GetProjectUserContextAsync(int projectId, string userId);
        Task<IEnumerable<TeamMemberResponse>> GetProjectMembersAsync(int projectId);
        Task<bool> RemoveMemberAsync(int projectId, int memberId, string removedBy);
    }
}
