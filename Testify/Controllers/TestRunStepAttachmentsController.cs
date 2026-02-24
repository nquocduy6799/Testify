using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Client.Interfaces;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestRunStepAttachments;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestRunStepAttachmentsController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ITestRunStepAttachmentRepository _attachmentRepository;
        private const long MaxFileSize = 10 * 1024 * 1024;

        public TestRunStepAttachmentsController(ICloudinaryService cloudinaryService, ITestRunStepAttachmentRepository attachmentRepository)
        {
            _cloudinaryService = cloudinaryService;
            _attachmentRepository = attachmentRepository;
        }

        [HttpPost("upload/{runStepId}")]
        public async Task<ActionResult<IEnumerable<TestRunStepAttachmentResponse>>> Upload(int runStepId, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var userName = User.Identity?.Name ?? "System";
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var uploadedAttachments = new List<TestRunStepAttachmentResponse>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    errors.Add($"Skipped empty file.");
                    continue;
                }

                if (file.Length > MaxFileSize)
                {
                    errors.Add($"File '{file.FileName}' exceeds maximum limit of {MaxFileSize / 1024 / 1024}MB.");
                    continue;
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    var (url, publicId) = await _cloudinaryService.UploadFileAsync(stream, file.FileName);

                    var request = new CreateTestRunStepAttachmentRequest
                    {
                        RunStepId = runStepId,
                        FileName = file.FileName,
                        FileUrl = url,
                        PublicId = publicId,
                        FileSize = file.Length,
                        ContentType = file.ContentType
                    };

                    var attachment = await _attachmentRepository.CreateAttachmentAsync(request, userName);

                    uploadedAttachments.Add(attachment);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to upload '{file.FileName}': {ex.Message}");
                }
            }

            if (uploadedAttachments.Count == 0)
            {
                return BadRequest(new
                {
                    message = "No files were uploaded successfully.",
                    errors
                });
            }

            if (errors.Count > 0)
            {
                return Ok(new
                {
                    attachments = uploadedAttachments,
                    partialSuccess = true,
                    errors
                });
            }

            return Ok(uploadedAttachments);
        }

        [HttpGet("run-step/{runStepId}")]
        public async Task<ActionResult<IEnumerable<TestRunStepAttachmentResponse>>> GetAttachments(int runStepId)
        {
            var attachments = await _attachmentRepository.GetAttachmentsByRunStepIdAsync(runStepId);
            return Ok(attachments);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
            if (attachment == null)
                return NotFound();

            // Delete from Cloudinary
            if (!string.IsNullOrEmpty(attachment.PublicId))
            {
                await _cloudinaryService.DeleteFileAsync(attachment.PublicId);
            }

            var userName = User.Identity?.Name ?? "System";
            var deleted = await _attachmentRepository.DeleteAttachmentAsync(id, userName);

            return deleted ? NoContent() : NotFound();
        }
    }
}

