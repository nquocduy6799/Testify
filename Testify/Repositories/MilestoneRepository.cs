using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.Milestones;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Repositories
{
    public class MilestoneRepository : IMilestoneRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IKanbanTaskRepository _kanbanTaskRepository;

        public MilestoneRepository(ApplicationDbContext context, IKanbanTaskRepository kanbanTaskRepository)
        {
            _context = context;
            _kanbanTaskRepository = kanbanTaskRepository;
        }

        public async Task<IEnumerable<MilestoneResponse>> GetMilestonesByProjectIdAsync(int projectId)
        {
            var milestones = await _context.Milestones
                .Where(m => m.ProjectId == projectId && !m.IsDeleted)
                .Include(m => m.TestPlans.Where(tp => !tp.IsDeleted))
                .OrderBy(m => m.StartDate)
                .ToListAsync();

            var response = new List<MilestoneResponse>();

            foreach (var milestone in milestones)
            {
                var tasks = await _kanbanTaskRepository.GetTasksByMilestoneIdAsync(milestone.Id);

                var mapped = MapToResponse(milestone, tasks);
                mapped.TestPlanCount = milestone.TestPlans?.Count ?? 0;
                response.Add(mapped);
            }

            return response;
        }

        public async Task<MilestoneResponse?> GetMilestoneByIdAsync(int id)
        {
            var milestone = await _context.Milestones
                .Include(m => m.TestPlans.Where(tp => !tp.IsDeleted))
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (milestone == null)
                return null;

            var tasks = await _kanbanTaskRepository.GetTasksByMilestoneIdAsync(milestone.Id);
            var response = MapToResponse(milestone, tasks);
            response.TestPlanCount = milestone.TestPlans?.Count ?? 0;
            return response;
        }

        public async Task<MilestoneResponse> CreateMilestoneAsync(CreateMilestoneRequest request, string userName)
        {
            var milestone = new Milestone
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status
            };

            milestone.MarkAsCreated(userName);

            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            return MapToResponse(milestone);
        }

        public async Task<bool> UpdateMilestoneAsync(int id, UpdateMilestoneRequest request, string userName)
        {
            var milestone = await _context.Milestones
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (milestone == null)
                return false;

            milestone.Name = request.Name;
            milestone.Description = request.Description;
            milestone.StartDate = request.StartDate;
            milestone.EndDate = request.EndDate;
            milestone.Status = request.Status;

            milestone.MarkAsUpdated(userName);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMilestoneAsync(int id, string userName)
        {
            var milestone = await _context.Milestones
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (milestone == null)
                return false;

            milestone.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MilestoneExistsAsync(int id)
        {
            return await _context.Milestones.AnyAsync(m => m.Id == id && !m.IsDeleted);
        }

        private static MilestoneResponse MapToResponse(Milestone milestone, IEnumerable<Shared.DTOs.KanbanTasks.KanbanTaskResponse>? tasks = null)
        {
            return new MilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                Description = milestone.Description,
                StartDate = milestone.StartDate,
                EndDate = milestone.EndDate,
                Status = milestone.Status,
                Tasks = tasks?.ToList() ?? new()
            };
        }
    }
}
