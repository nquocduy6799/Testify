using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestRunStepAttachments;

namespace Testify.Repositories
{
    public class TestRunStepAttachmentRepository : ITestRunStepAttachmentRepository
    {
        private readonly ApplicationDbContext _context;

        public TestRunStepAttachmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AttachmentExistsAsync(int attachmentId)
        {
            return await _context.TestRunStepAttachments
                .AnyAsync(a => a.Id == attachmentId && !a.IsDeleted);
        }

        public async Task<TestRunStepAttachmentResponse> CreateAttachmentAsync(
            CreateTestRunStepAttachmentRequest request,
            string userName)
        {
            var attachment = new TestRunStepAttachment
            {
                RunStepId = request.RunStepId,
                FileName = request.FileName,
                PublicId = request.PublicId,
                FileUrl = request.FileUrl,
                FileSize = request.FileSize,
                ContentType = request.ContentType
            };

            attachment.MarkAsCreated(userName);

            _context.TestRunStepAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return MapToResponse(attachment);
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId, string userName)
        {
            var attachment = await _context.TestRunStepAttachments.FindAsync(attachmentId);

            if (attachment == null || attachment.IsDeleted)
                return false;

            attachment.MarkAsDeleted(userName);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TestRunStepAttachmentResponse?> GetAttachmentByIdAsync(int attachmentId)
        {
            var attachment = await _context.TestRunStepAttachments
                .Where(a => a.Id == attachmentId && !a.IsDeleted)
                .FirstOrDefaultAsync();

            return attachment != null ? MapToResponse(attachment) : null;
        }

        public async Task<int> GetAttachmentCountByRunStepIdAsync(int runStepId)
        {
            return await _context.TestRunStepAttachments
                .CountAsync(a => a.RunStepId == runStepId && !a.IsDeleted);
        }

        public async Task<IEnumerable<TestRunStepAttachmentResponse>> GetAttachmentsByRunStepIdAsync(int runStepId)
        {
            return await _context.TestRunStepAttachments
                .Where(a => a.RunStepId == runStepId && !a.IsDeleted)
                .Select(a => MapToResponse(a))
                .ToListAsync();
        }

        public async Task<TestRunStepAttachmentResponse> UpdateAttachmentAsync(
            int id,
            UpdateTestRunStepAttachmentRequest request,
            string userName)
        {
            var attachment = await _context.TestRunStepAttachments.FindAsync(id);

            if (attachment == null)
                throw new InvalidOperationException("Attachment not found");

            attachment.RunStepId = request.RunStepId;
            attachment.FileName = request.FileName;
            attachment.PublicId = request.PublicId;
            attachment.FileUrl = request.FileUrl;
            attachment.FileSize = request.FileSize;
            attachment.ContentType = request.ContentType;
            attachment.MarkAsUpdated(userName);

            await _context.SaveChangesAsync();

            return MapToResponse(attachment);
        }

        private static TestRunStepAttachmentResponse MapToResponse(TestRunStepAttachment attachment)
        {
            return new TestRunStepAttachmentResponse
            {
                Id = attachment.Id,
                RunStepId = attachment.RunStepId,
                FileName = attachment.FileName,
                PublicId = attachment.PublicId ?? string.Empty,
                FileUrl = attachment.FileUrl,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                CreatedAt = attachment.CreatedAt,
                CreatedBy = attachment.CreatedBy,
                UpdatedAt = attachment.UpdatedAt,
                UpdatedBy = attachment.UpdatedBy
            };
        }
    }
}