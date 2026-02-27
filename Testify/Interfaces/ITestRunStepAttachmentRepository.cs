using Testify.Shared.DTOs.TaskAttachments;
using Testify.Shared.DTOs.TestRunStepAttachments;

namespace Testify.Interfaces
{
    public interface ITestRunStepAttachmentRepository
    {
        Task<TestRunStepAttachmentResponse?> GetAttachmentByIdAsync(int attachmentId);
        Task<IEnumerable<TestRunStepAttachmentResponse>> GetAttachmentsByRunStepIdAsync(int runStepId);
        Task<TestRunStepAttachmentResponse> CreateAttachmentAsync(CreateTestRunStepAttachmentRequest request, string userName);
        Task<TestRunStepAttachmentResponse> UpdateAttachmentAsync(int id, UpdateTestRunStepAttachmentRequest request, string userName);
        Task<bool> DeleteAttachmentAsync(int attachmentId, string userName);
        Task<bool> AttachmentExistsAsync(int attachmentId);
        Task<int> GetAttachmentCountByRunStepIdAsync(int runStepId);
    }
}
