using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Milestones;

namespace Testify.Client.Interfaces
{
    public interface IKanbanTaskService
    {
        Task<List<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId);
        Task<List<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId);
        Task<KanbanTaskResponse> GetTaskByIdAsync(int id);
        Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request);
        Task<KanbanTaskResponse> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);
    }
}