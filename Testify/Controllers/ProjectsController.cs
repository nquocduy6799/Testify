using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController(
        IProjectRepository projectRepository,
        IHubContext<NotificationHub> hubContext) : ControllerBase
    {
        /// <summary>
        /// Retrieves all projects that belong to the authenticated user.
        /// </summary>
        /// <returns>A list of projects associated with the current user.</returns>
        /// <response code="200">Returns the list of user projects.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var projects = await projectRepository.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a specific project by its ID.
        /// </summary>
        /// <param name="id">The ID of the project to retrieve.</param>
        /// <returns>The project with the specified ID.</returns>
        /// <response code="200">Returns the requested project.</response>
        /// <response code="404">Project with the given ID was not found.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var project = await projectRepository.GetProjectByIdAsync(id)
                ?? throw new NotFoundException($"Project with id {id} was not found.");

            return Ok(project);
        }

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        /// <param name="id">The ID of the project to update.</param>
        /// <param name="request">The updated project data.</param>
        /// <returns>No content if the update is successful.</returns>
        /// <response code="204">Project updated successfully.</response>
        /// <response code="404">Project with the given ID was not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, UpdateProjectRequest request)
        {
            var userName = User.Identity?.Name ?? "System";
            var updated = await projectRepository.UpdateProjectAsync(id, request, userName);

            if (!updated)
                throw new NotFoundException($"Project with id {id} was not found.");

            return NoContent();
        }

        /// <summary>
        /// Creates a new project for the authenticated user.
        /// </summary>
        /// <param name="request">The project creation data.</param>
        /// <returns>The newly created project.</returns>
        /// <response code="201">Project created successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> PostProject(CreateProjectRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var userName = User.Identity?.Name ?? "System";
            var project = await projectRepository.CreateProjectAsync(request, userId, userName);

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns>No content if deletion is successful.</returns>
        /// <response code="204">Project deleted successfully.</response>
        /// <response code="404">Project with the given ID was not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await projectRepository.DeleteProjectAsync(id, userName);

            if (!deleted)
                throw new NotFoundException($"Project with id {id} was not found.");

            return NoContent();
        }

        /// <summary>
        /// Retrieves the authenticated user's role within a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>The user's role in the project.</returns>
        /// <response code="200">Returns the user's project role.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{projectId}/my-role")]
        public async Task<ActionResult<ProjectRole?>> GetMyRoleInProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var role = await projectRepository.GetUserRoleInProjectAsync(projectId, userId);
            return Ok(role);
        }

        /// <summary>
        /// Retrieves contextual information about the authenticated user's involvement in a project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>The user's project context.</returns>
        /// <response code="200">Returns the project user context.</response>
        /// <response code="404">Project context was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{projectId}/my-context")]
        public async Task<ActionResult<ProjectUserContext?>> GetProjectUserContext(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var context = await projectRepository.GetProjectUserContextAsync(projectId, userId)
                ?? throw new NotFoundException($"Project context for project {projectId} was not found.");

            return Ok(context);
        }

        /// <summary>
        /// Retrieves all members of a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>A list of project members.</returns>
        /// <response code="200">Returns the list of project members.</response>
        [HttpGet("{projectId}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberResponse>>> GetProjectMembers(int projectId)
        {
            var members = await projectRepository.GetProjectMembersAsync(projectId);
            return Ok(members);
        }

        /// <summary>
        /// Removes a member from a project. Only Project Managers can perform this action.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="memberId">The ID of the member to remove.</param>
        /// <returns>A success message if the member is removed.</returns>
        /// <response code="200">Member removed successfully.</response>
        /// <response code="403">User is not authorized to remove members.</response>
        /// <response code="404">Member was not found or cannot be removed.</response>
        [HttpDelete("{projectId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int projectId, int memberId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var currentUserRole = await projectRepository.GetUserRoleInProjectAsync(projectId, currentUserId);
            if (currentUserRole != ProjectRole.PM)
                throw new ForbiddenException("Only Project Managers can remove members.");

            var members = await projectRepository.GetProjectMembersAsync(projectId);
            var memberToRemove = members.FirstOrDefault(m => m.Id == memberId);

            var removed = await projectRepository.RemoveMemberAsync(projectId, memberId, currentUserId);
            if (!removed)
                throw new NotFoundException($"Member with id {memberId} was not found or cannot be removed.");

            if (memberToRemove != null)
            {
                await hubContext.Clients.All.SendAsync("TeamMemberRemoved", projectId, memberToRemove.UserId);
                Console.WriteLine($"[ProjectsController] Broadcast TeamMemberRemoved - Project {projectId}, User {memberToRemove.UserId}");
            }

            return Ok(new { message = "Member removed successfully" });
        }
    }
}
