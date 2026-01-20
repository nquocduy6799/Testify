using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface IKanbanTaskRepository
    {
        Task<IEnumerable<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId);
        Task<KanbanTaskResponse?> GetTaskByTaskIdAsync(int id);
        Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request, string userId, string userName);
        Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request, string userName);
        Task<bool> DeleteTaskAsync(int id, string userName);
    }
}

