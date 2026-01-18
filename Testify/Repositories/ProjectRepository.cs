using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectResponse>> GetUserProjectsAsync(string userId)
        {
            var projects = await _context.Projects
                .Include(p => p.TeamMembers)
                .Where(p => !p.IsDeleted && p.TeamMembers.Any(tm => tm.UserId == userId))
                .ToListAsync();

            return projects.Select(p => MapToResponse(p)).ToList();
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
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
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string userId, string userName)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                Client = request.Client,
                Deadline = request.Deadline
            };

            project.MarkAsCreated(userName);

            // Add creator as team member
            project.TeamMembers = new List<ProjectTeamMember>
    {
        new ProjectTeamMember
        {
            UserId = userId,
            Role = ProjectRole.PM, 
            JoinedAt = DateTimeHelper.GetVietnamTime()
        }
    };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return MapToResponse(project);
        }

        public async Task<bool> UpdateProjectAsync(int id, UpdateProjectRequest request, string userName)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null || project.IsDeleted)
                return false;

            project.Name = request.Name;
            project.Description = request.Description;
            project.Client = request.Client;
            project.Progress = request.Progress;
            project.Deadline = request.Deadline;
            project.BugThreshold = request.BugThreshold;
            project.MarkAsUpdated(userName);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExistsAsync(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id, string userName)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null || project.IsDeleted)
                return false;

            project.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ProjectExistsAsync(int id)
        {
            return await _context.Projects.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        private static ProjectResponse MapToResponse(Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
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
        }

        public async Task<ProjectRole?> GetUserRoleInProjectAsync(int projectId, string userId)
        {
            var teamMember = await _context.ProjectTeamMembers
                .Where(tm => tm.ProjectId == projectId && tm.UserId == userId)
                .FirstOrDefaultAsync();

            return teamMember?.Role;
        }
    }
}