using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.DTOs.Projects;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var projects = await _context.Projects
                .Where(p => !p.IsDeleted)
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Color = p.Color,
                    Client = p.Client,
                    Progress = p.Progress,
                    Deadline = p.Deadline,
                    MembersCount = p.MembersCount,
                    BugThreshold = p.BugThreshold,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedAt = p.UpdatedAt,
                    UpdatedBy = p.UpdatedBy
                })
                .ToListAsync();

            return projects;
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var project = await _context.Projects
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Color = p.Color,
                    Client = p.Client,
                    Progress = p.Progress,
                    Deadline = p.Deadline,
                    MembersCount = p.MembersCount,
                    BugThreshold = p.BugThreshold,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedAt = p.UpdatedAt,
                    UpdatedBy = p.UpdatedBy
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, UpdateProjectRequest request)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null || project.IsDeleted)
            {
                return NotFound();
            }

            // Map DTO to entity
            project.Name = request.Name;
            project.Description = request.Description;
            project.Color = request.Color;
            project.Client = request.Client;
            project.Progress = request.Progress;
            project.Deadline = request.Deadline;
            project.BugThreshold = request.BugThreshold;

            // Update audit fields
            project.MarkAsUpdated(User.Identity?.Name ?? "System");

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> PostProject(CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                Client = request.Client,
                Deadline = request.Deadline,
                BugThreshold = request.BugThreshold
            };

            // Set audit fields
            project.MarkAsCreated(User.Identity?.Name ?? "System");

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Color = project.Color,
                Client = project.Client,
                Progress = project.Progress,
                Deadline = project.Deadline,
                MembersCount = project.MembersCount,
                BugThreshold = project.BugThreshold,
                CreatedAt = project.CreatedAt,
                CreatedBy = project.CreatedBy,
                UpdatedAt = project.UpdatedAt,
                UpdatedBy = project.UpdatedBy
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null || project.IsDeleted)
            {
                return NotFound();
            }

            // Soft delete
            project.MarkAsDeleted(User.Identity?.Name ?? "System");
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}