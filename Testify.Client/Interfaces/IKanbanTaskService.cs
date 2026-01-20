using Testify.Shared.DTOs.KanbanTasks;

namespace Testify.Client.Interfaces
{
    public interface IKanbanTaskService
    {
        Task<List<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId);
        Task<KanbanTaskResponse> GetTaskByIdAsync(int id);
        Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request);
        Task<KanbanTaskResponse> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);
    }
}