using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.DTOs.TaskAttachments;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface ITaskAttachmentRepository
    {
        Task<TaskAttachmentResponse?> GetAttachmentByIdAsync(int attachmentId);
        Task<IEnumerable<TaskAttachmentResponse>> GetAttachmentsByTaskIdAsync(int kanbanTaskId);
        Task<TaskAttachmentResponse> CreateAttachmentAsync(CreateTaskAttachmentRequest request, string userName);
        Task<TaskAttachmentResponse> UpdateAttachmentAsync(int id ,UpdateTaskAttachmentRequest request, string userName);
        Task<bool> DeleteAttachmentAsync(int attachmentId, string userName);
        Task<bool> AttachmentExistsAsync(int attachmentId);
        Task<int> GetAttachmentCountByTaskIdAsync(int kanbanTaskId);
    }
}
