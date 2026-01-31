using Microsoft.AspNetCore.Components.Forms;
using Testify.Shared.DTOs.TaskAttachments;

namespace Testify.Client.Interfaces
{
    public interface ITaskAttachmentService
    {
        Task<List<TaskAttachmentResponse>> UploadAttachmentsAsync(int kanbanTaskId, List<IBrowserFile> files);
        Task<List<TaskAttachmentResponse>> GetAttachmentsByTaskIdAsync(int kanbanTaskId);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
    }
}
