using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TaskActivity;
using Testify.Shared.DTOs.TaskAttachments;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface IKanbanTaskRepository
    {
        Task<IEnumerable<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId);
        Task<IEnumerable<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId);
        Task<KanbanTaskResponse?> GetTaskByTaskIdAsync(int id);
        Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request, string userName, string userId);
        Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request, string userName, string userId);
        Task<bool> DeleteTaskAsync(int id, string userName);
        Task<IEnumerable<TaskAttachmentResponse>> GetTaskAttachmentsAsync(int taskId);
        Task<List<TaskActivityResponse>> GetTaskActivityResponsesAsync(int taskId);
    }
}

