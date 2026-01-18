using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        public ProjectsController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
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
    }
}