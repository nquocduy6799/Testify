using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Milestones;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Repositories
{
    public class KanbanTaskRepository : IKanbanTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public KanbanTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId)
        {
            return await _context.KanbanTasks
                .Where(t => t.MilestoneId == milestoneId && !t.IsDeleted)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .Select(t => MapToResponse(t))
                .ToListAsync();
        }

        public async Task<KanbanTaskResponse?> GetTaskByTaskIdAsync(int id)
        {
            var task = await _context.KanbanTasks
                .Where(t => t.Id == id && !t.IsDeleted)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .FirstOrDefaultAsync();

            return task != null ? MapToResponse(task) : null;
        }

        public async Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request, string userId, string userName)
        {
            var task = new KanbanTask
            {
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                AssigneeId = request.AssigneeId,
                Type = request.Type
            };

            task.MarkAsCreated(userName);

            _context.KanbanTasks.Add(task);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(task)
                .Reference(t => t.Assignee)
                .LoadAsync();

            await _context.Entry(task)
                .Collection(t => t.TestPlans)
                .LoadAsync();

            return MapToResponse(task);
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request, string userName)
        {
            var task = await _context.KanbanTasks.FindAsync(id);

            if (task == null || task.IsDeleted)
                return false;

            task.MilestoneId = request.MilestoneId;
            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.AssigneeId = request.AssigneeId;
            task.Type = request.Type;
            task.MarkAsUpdated(userName);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExistsAsync(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id, string userName)
        {
            var task = await _context.KanbanTasks.FindAsync(id);

            if (task == null || task.IsDeleted)
                return false;

            task.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> TaskExistsAsync(int id)
        {
            return await _context.KanbanTasks.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }

        private static KanbanTaskResponse MapToResponse(KanbanTask task)
        {
            return new KanbanTaskResponse
            {
                Id = task.Id,
                MilestoneId = task.MilestoneId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.UserName,
                AssigneeAvatarUrl = task.Assignee?.AvatarUrl,
                Type = task.Type,
                CreatedAt = task.CreatedAt,
                CreatedBy = task.CreatedBy,
                UpdatedAt = task.UpdatedAt,
                TestPlanCount = task.TestPlans?.Count ?? 0
            };
        }

        public async Task<IEnumerable<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _context.KanbanTasks
                .Where(t => t.Milestone.ProjectId == projectId && !t.IsDeleted && t.Milestone.Status == MilestoneStatus.Active)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .Select(t => MapToResponse(t))
                .ToListAsync();
        }
    }
}