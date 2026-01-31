using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TaskAttachments;

namespace Testify.Repositories
{
    public class TaskAttachmentRepository : ITaskAttachmentRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskAttachmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskAttachmentResponse?> GetAttachmentByIdAsync(int attachmentId)
        {
            var attachment = await _context.TaskAttachments
                .Where(a => a.Id == attachmentId && !a.IsDeleted)
                .FirstOrDefaultAsync();

            return attachment != null ? MapToResponse(attachment) : null;
        }

        public async Task<IEnumerable<TaskAttachmentResponse>> GetAttachmentsByTaskIdAsync(int kanbanTaskId)
        {
            return await _context.TaskAttachments
                .Where(a => a.KanbanTaskId == kanbanTaskId && !a.IsDeleted)
                .Select(a => MapToResponse(a))
                .ToListAsync();
        }

        public async Task<TaskAttachmentResponse> CreateAttachmentAsync(CreateTaskAttachmentRequest request, string userName)
        {
            var attachment = new TaskAttachment
            {
                KanbanTaskId = request.KanbanTaskId,
                FileName = request.FileName,
                FileUrl = request.FileUrl,
                PublicId = request.PublicId,
                FileSize = request.FileSize,
                ContentType = request.ContentType
            };

            attachment.MarkAsCreated(userName);

            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToResponse(attachment);
        }

        public async Task<TaskAttachmentResponse> UpdateAttachmentAsync(int id, UpdateTaskAttachmentRequest request, string userName)
        {
            var attachment = await _context.TaskAttachments.FindAsync(id);

            if (attachment == null)
                throw new InvalidOperationException("Attachment not found");

            attachment.FileName = request.FileName;
            attachment.FileUrl = request.FileUrl;
            attachment.PublicId = request.PublicId;
            attachment.FileSize = request.FileSize;
            attachment.ContentType = request.ContentType;
            attachment.MarkAsUpdated(userName);

            await _context.SaveChangesAsync();

            return MapToResponse(attachment);
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId, string deletedBy)
        {
            var attachment = await _context.TaskAttachments.FindAsync(attachmentId);

            if (attachment == null || attachment.IsDeleted)
                return false;

            attachment.MarkAsDeleted(deletedBy);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AttachmentExistsAsync(int attachmentId)
        {
            return await _context.TaskAttachments
                .AnyAsync(a => a.Id == attachmentId && !a.IsDeleted);
        }

        public async Task<int> GetAttachmentCountByTaskIdAsync(int kanbanTaskId)
        {
            return await _context.TaskAttachments
                .CountAsync(a => a.KanbanTaskId == kanbanTaskId && !a.IsDeleted);
        }

        private static TaskAttachmentResponse MapToResponse(TaskAttachment attachment)
        {
            return new TaskAttachmentResponse
            {
                Id = attachment.Id,
                KanbanTaskId = attachment.KanbanTaskId,
                FileName = attachment.FileName,
                FileUrl = attachment.FileUrl,
                PublicId = attachment.PublicId ?? string.Empty,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType
            };
        }
    }
}