using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Milestones;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Client.Interfaces
{
    public interface IKanbanTaskService
    {
        Task<List<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId);
        Task<List<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId);
        Task<KanbanTaskResponse> GetTaskByIdAsync(int id);
        Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request);
        Task<KanbanTaskResponse> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request);
        Task<List<TaskActivityResponse>> GetTaskActivityResponsesAsync(int taskId);
        Task<bool> DeleteTaskAsync(int id);
    }
}