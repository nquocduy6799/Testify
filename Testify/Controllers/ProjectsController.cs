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
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ProjectsController(IProjectRepository projectRepository, IHubContext<NotificationHub> hubContext)
        {
            _projectRepository = projectRepository;
            _hubContext = hubContext;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var projects = await _projectRepository.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, UpdateProjectRequest request)
        {
            var userName = User.Identity?.Name ?? "System";
            var updated = await _projectRepository.UpdateProjectAsync(id, request, userName);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> PostProject(CreateProjectRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "System";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var project = await _projectRepository.CreateProjectAsync(request, userId, userName);

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await _projectRepository.DeleteProjectAsync(id, userName);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/Projects/5/my-role
        [HttpGet("{projectId}/my-role")]
        public async Task<ActionResult<ProjectRole?>> GetMyRoleInProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var role = await _projectRepository.GetUserRoleInProjectAsync(projectId, userId);

            return Ok(role);
        }


        [HttpGet("{projectId}/my-context")]
        public async Task<ActionResult<ProjectUserContext?>> GetProjectUserContext(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var context = await _projectRepository.GetProjectUserContextAsync(projectId, userId);

            if (context == null)
            {
                return NotFound();
            }

            return Ok(context);
        }


        // GET: api/Projects/5/members
        [HttpGet("{projectId}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberResponse>>> GetProjectMembers(int projectId)
        {
            var members = await _projectRepository.GetProjectMembersAsync(projectId);
            return Ok(members);
        }

        // DELETE: api/Projects/5/members/{memberId}
        [HttpDelete("{projectId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int projectId, int memberId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            // Check if current user is PM
            var currentUserRole = await _projectRepository.GetUserRoleInProjectAsync(projectId, currentUserId);
            if (currentUserRole != ProjectRole.PM)
            {
                return StatusCode(403, new { message = "Only Project Managers can remove members" });
            }

            // Get member info before removing
            var members = await _projectRepository.GetProjectMembersAsync(projectId);
            var memberToRemove = members.FirstOrDefault(m => m.Id == memberId);
            
            var removed = await _projectRepository.RemoveMemberAsync(projectId, memberId, currentUserId);

            if (!removed)
            {
                return NotFound(new { message = "Member not found or cannot be removed" });
            }

            // Broadcast team member removed via SignalR
            if (memberToRemove != null)
            {
                await _hubContext.Clients.All.SendAsync("TeamMemberRemoved", projectId, memberToRemove.UserId);
                Console.WriteLine($"[ProjectsController] Broadcast TeamMemberRemoved - Project {projectId}, User {memberToRemove.UserId}");
            }

            return Ok(new { message = "Member removed successfully" });
        }
    }
}