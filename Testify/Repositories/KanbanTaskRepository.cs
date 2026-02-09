using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Interfaces.Testify.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Milestones;
using Testify.Shared.DTOs.TaskActivity;
using Testify.Shared.DTOs.TaskAttachments;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Repositories
{
    public class KanbanTaskRepository : IKanbanTaskRepository
    {
        private readonly ApplicationDbContext _context;
        private ITaskActivityRepository _taskActivityRepository;

        public KanbanTaskRepository(ApplicationDbContext context, ITaskActivityRepository taskActivityRepository)
        {
            _context = context;
            _taskActivityRepository = taskActivityRepository;
        }

        public async Task<IEnumerable<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId)
        {
            return await _context.KanbanTasks
                .Where(t => t.MilestoneId == milestoneId && !t.IsDeleted)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .Include(t => t.Attachments)
                .Select(t => MapToResponse(t))
                .ToListAsync();
        }

        public async Task<KanbanTaskResponse?> GetTaskByTaskIdAsync(int id)
        {
            var task = await _context.KanbanTasks
                .Where(t => t.Id == id && !t.IsDeleted)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .Include(t => t.Attachments)
 
                .FirstOrDefaultAsync();

            return task != null ? MapToResponse(task) : null;
        }

        public async Task<IEnumerable<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _context.KanbanTasks
                .Where(t => t.Milestone.ProjectId == projectId && !t.IsDeleted && t.Milestone.Status == MilestoneStatus.Active)
                .Include(t => t.Assignee)
                .Include(t => t.TestPlans)
                .Include(t => t.Attachments)
                .Include(t => t.Activities)
                .Select(t => MapToResponse(t))
                .ToListAsync();
        }

        //public async Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request, string userName)
        //{
        //    var task = new KanbanTask
        //    {
        //        MilestoneId = request.MilestoneId,
        //        Title = request.Title,
        //        Description = request.Description,
        //        DueDate = request.DueDate,
        //        Status = request.Status,
        //        Priority = request.Priority,
        //        AssigneeId = request.AssigneeId,
        //        Type = request.Type
        //    };

        //    task.MarkAsCreated(userName);

        //    _context.KanbanTasks.Add(task);
        //    await _context.SaveChangesAsync();

        //    // Reload with navigation properties
        //    await _context.Entry(task)
        //        .Reference(t => t.Assignee)
        //        .LoadAsync();

        //    await _context.Entry(task)
        //        .Collection(t => t.TestPlans)
        //        .LoadAsync();

        //    return MapToResponse(task);
        //}


        public async Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request, string userName, string userId)
        {
            var task = new KanbanTask
            {
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = request.Status,
                Priority = request.Priority,
                AssigneeId = request.AssigneeId,
                Type = request.Type
            };

            task.MarkAsCreated(userName);

            _context.KanbanTasks.Add(task);
            await _context.SaveChangesAsync();

            // Log task creation activity
            await _taskActivityRepository.RecordTaskCreationAsync(task, userName, userId);

            // Reload with navigation properties
            await _context.Entry(task)
                .Reference(t => t.Assignee)
                .LoadAsync();

            await _context.Entry(task)
                .Collection(t => t.TestPlans)
                .LoadAsync();

            return MapToResponse(task);
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request, string userName, string userId)
        {
            // Get the original task state for activity tracking
            var originalTask = await _context.KanbanTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (originalTask == null || originalTask.IsDeleted)
                return false;

            // Get the task for updating
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

                await _taskActivityRepository.RecordTaskUpdateAsync(originalTask, task, userName, userId);

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
                DueDate = task.DueDate,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.FullName,
                AssigneeAvatarUrl = task.Assignee?.AvatarUrl,
                Type = task.Type,
                CreatedAt = task.CreatedAt,
                CreatedBy = task.CreatedBy,
                UpdatedAt = task.UpdatedAt,
                TestPlanCount = task.TestPlans?.Count ?? 0,
                Attachments = task.Attachments.Select(a => new TaskAttachmentResponse
                {
                    Id = a.Id,
                    KanbanTaskId = a.KanbanTaskId,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    PublicId = a.PublicId ?? string.Empty,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType
                }).ToList(),
                Activities = task.Activities.Select(act => new TaskActivityResponse
                {
                    Id = act.Id,
                    KanbanTaskId = act.KanbanTaskId,
                    FullName = act.FullName,
                    Action = act.Action,
                    OldValue = act.OldValue,
                    NewValue = act.NewValue,
                    Description = act.Description,
                    CreatedBy = act.CreatedBy,
                    CreatedAt = act.CreatedAt
                }).ToList()
            };
        }
    }
}
