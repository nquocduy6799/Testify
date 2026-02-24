using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces.Testify.Interfaces;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Repositories
{
    public class TaskActivityRepository : ITaskActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        async Task<TaskActivity> ITaskActivityRepository.CreateActivityAsync(TaskActivity activity)
        {
            _context.TaskActivities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        async Task<IEnumerable<TaskActivityResponse>> ITaskActivityRepository.GetActivitiesByTaskIdAsync(int taskId)
        {
            return await _context.TaskActivities
                .Where(a => a.KanbanTaskId == taskId)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new TaskActivityResponse
                {
                    Id = a.Id,
                    KanbanTaskId = a.KanbanTaskId,
                    FullName = a.User.FullName ?? string.Empty,
                    Action = a.Action,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    Description = a.Description,
                    CreatedBy = a.CreatedBy,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        async Task<bool> ITaskActivityRepository.RecordTaskCreationAsync(KanbanTask kanbanTask, string userName, string userId)
        {
            if (kanbanTask == null)
            {
                return false;
            }

            var fullName = _context.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefault() ?? string.Empty;
            var activity = new TaskActivity
            {
                KanbanTaskId = kanbanTask.Id,
                UserId = userId,
                FullName = fullName,
                Action = "Created",
                OldValue = null,
                NewValue = kanbanTask.Title,
                Description = $"Task '{kanbanTask.Title}' created by {fullName}.",
                CreatedBy = userName
            };

            _context.TaskActivities.Add(activity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        async Task<bool> ITaskActivityRepository.RecordTaskUpdateAsync(KanbanTask originalTask, KanbanTask updatedTask, string userName, string userId)
        {
            if (originalTask == null || updatedTask == null)
            {
                return false;
            }

            var fullName = _context.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefault() ?? string.Empty;
            var activities = new List<TaskActivity>();

            // Track Title changes
            if (originalTask.Title != updatedTask.Title)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Updated",
                    OldValue = originalTask.Title,
                    NewValue = updatedTask.Title,
                    Description = $"Changed title from '{originalTask.Title}' to '{updatedTask.Title}'",
                    CreatedBy = userName
                });
            }

            // Track Description changes
            if (originalTask.Description != updatedTask.Description)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Updated",
                    OldValue = originalTask.Description,
                    NewValue = updatedTask.Description,
                    Description = $"Updated description from '{originalTask.Description}' to '{updatedTask.Description}'",
                    CreatedBy = userName
                });
            }

            // Track Status changes
            if (originalTask.Status != updatedTask.Status)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Status Changed",
                    OldValue = originalTask.Status.ToString(),
                    NewValue = updatedTask.Status.ToString(),
                    Description = $"Changed status from '{originalTask.Status}' to '{updatedTask.Status}'",
                    CreatedBy = userName
                });
            }

            // Track Priority changes
            if (originalTask.Priority != updatedTask.Priority)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Priority Changed",
                    OldValue = originalTask.Priority.ToString(),
                    NewValue = updatedTask.Priority.ToString(),
                    Description = $"Changed priority from '{originalTask.Priority}' to '{updatedTask.Priority}'",
                    CreatedBy = userName
                });
            }

            // Track Assignee changes
            if (originalTask.AssigneeId != updatedTask.AssigneeId)
            {
                var newAssigneeName = updatedTask.AssigneeId != null
                    ? _context.Users.Where(u => u.Id == updatedTask.AssigneeId).Select(u => u.FullName).FirstOrDefault() ?? "Unassigned"
                    : "Unassigned";

                var oldAssigneeName = originalTask.AssigneeId != null
                    ? _context.Users.Where(u => u.Id == originalTask.AssigneeId).Select(u => u.FullName).FirstOrDefault() ?? "Unassigned"
                    : "Unassigned";

                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Assignee Changed",
                    OldValue = oldAssigneeName,
                    NewValue = newAssigneeName,
                    Description = $"Changed assignee from '{oldAssigneeName}' to '{newAssigneeName}'",
                    CreatedBy = userName
                });
            }

            // Track Type changes
            if (originalTask.Type != updatedTask.Type)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Type Changed",
                    OldValue = originalTask.Type.ToString(),
                    NewValue = updatedTask.Type.ToString(),
                    Description = $"Changed type from '{originalTask.Type}' to '{updatedTask.Type}'",
                    CreatedBy = userName
                });
            }

            // Track Milestone changes
            if (originalTask.MilestoneId != updatedTask.MilestoneId)
            {
                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Milestone Changed",
                    OldValue = originalTask.MilestoneId.ToString(),
                    NewValue = updatedTask.MilestoneId.ToString(),
                    Description = $"Moved to different milestone",
                    CreatedBy = userName
                });
            }

            // Track DueDate changes
            if (originalTask.DueDate != updatedTask.DueDate)
            {
                var oldDate = originalTask.DueDate?.ToString("yyyy-MM-dd") ?? "No due date";
                var newDate = updatedTask.DueDate?.ToString("yyyy-MM-dd") ?? "No due date";

                activities.Add(new TaskActivity
                {
                    KanbanTaskId = updatedTask.Id,
                    UserId = userId,
                    FullName = fullName,
                    Action = "Due Date Changed",
                    OldValue = oldDate,
                    NewValue = newDate,
                    Description = $"Changed due date from '{oldDate}' to '{newDate}'",
                    CreatedBy = userName
                });
            }

            // Only save if there are changes
            if (activities.Any())
            {
                _context.TaskActivities.AddRange(activities);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }

            return true;
        }
    }
}