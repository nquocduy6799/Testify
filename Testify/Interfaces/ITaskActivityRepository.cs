using Testify.Entities;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Interfaces
{
    namespace Testify.Interfaces
    {
        public interface ITaskActivityRepository
        {
            Task<IEnumerable<TaskActivityResponse>> GetActivitiesByTaskIdAsync(int taskId);
            Task<TaskActivity> CreateActivityAsync(TaskActivity activity);
            Task<bool> RecordTaskCreationAsync(KanbanTask kanbanTask, string userName, string userId);
            Task<bool> RecordTaskUpdateAsync(KanbanTask originalTask, KanbanTask updatedTask, string userName, string userId);
        }
    }
}
