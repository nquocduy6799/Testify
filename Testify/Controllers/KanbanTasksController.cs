using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Services;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.TaskActivity;
using Testify.Shared.DTOs.TaskAttachments;

namespace Testify.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing Kanban tasks, including their attachments,
    /// activity history, and PDF audit trail exports.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KanbanTasksController(
        IKanbanTaskRepository kanbanTaskRepository,
        ILogger<KanbanTasksController> logger) : ControllerBase
    {
        #region Kanban Tasks

        /// <summary>
        /// Retrieves all Kanban tasks associated with a specific milestone.
        /// </summary>
        /// <param name="milestoneId">The ID of the milestone whose tasks are to be retrieved.</param>
        /// <returns>A list of Kanban tasks belonging to the specified milestone.</returns>
        /// <response code="200">Returns the list of tasks for the milestone.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("milestone/{milestoneId}")]
        [ProducesResponseType(typeof(IEnumerable<KanbanTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<KanbanTaskResponse>>> GetTasksByMilestone(int milestoneId)
        {
            var tasks = await kanbanTaskRepository.GetTasksByMilestoneIdAsync(milestoneId);
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves all Kanban tasks associated with a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose tasks are to be retrieved.</param>
        /// <returns>A list of Kanban tasks belonging to the specified project.</returns>
        /// <response code="200">Returns the list of tasks for the project.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("project/{projectId}")]
        [ProducesResponseType(typeof(IEnumerable<KanbanTaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<KanbanTaskResponse>>> GetTasksByProject(int projectId)
        {
            var tasks = await kanbanTaskRepository.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves a specific Kanban task by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>The Kanban task with the specified ID.</returns>
        /// <response code="200">Returns the requested task.</response>
        /// <response code="404">Task with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(KanbanTaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<KanbanTaskResponse>> GetTask(int id)
        {
            var task = await kanbanTaskRepository.GetTaskByTaskIdAsync(id);
            if (task is null)
            {
                logger.LogWarning("Kanban task {TaskId} was not found", id);
                throw new NotFoundException($"Kanban task with ID {id} was not found.");
            }

            return Ok(task);
        }

        /// <summary>
        /// Creates a new Kanban task.
        /// </summary>
        /// <param name="request">The data required to create the task.</param>
        /// <returns>The newly created Kanban task.</returns>
        /// <response code="201">Task created successfully. Returns the created task.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(KanbanTaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<KanbanTaskResponse>> PostTask(CreateKanbanTaskRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var userName = User.Identity?.Name ?? "System";

            var task = await kanbanTaskRepository.CreateTaskAsync(request, userName, userId);

            logger.LogInformation("Kanban task {TaskId} created by user {UserId}", task.Id, userId);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /// <summary>
        /// Updates an existing Kanban task.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="request">The updated task data.</param>
        /// <returns>No content if the update is successful.</returns>
        /// <response code="204">Task updated successfully.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="404">Task with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutTask(int id, UpdateKanbanTaskRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var userName = User.Identity?.Name ?? "System";

            var updated = await kanbanTaskRepository.UpdateTaskAsync(id, request, userName, userId);

            if (!updated)
            {
                logger.LogWarning("Kanban task {TaskId} was not found for update", id);
                throw new NotFoundException($"Kanban task with ID {id} was not found.");
            }

            logger.LogInformation("Kanban task {TaskId} updated successfully", id);

            return NoContent();
        }

        /// <summary>
        /// Deletes a Kanban task by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        /// <response code="204">Task deleted successfully.</response>
        /// <response code="404">Task with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userName = User.Identity?.Name ?? "System";

            var deleted = await kanbanTaskRepository.DeleteTaskAsync(id, userName);

            if (!deleted)
            {
                logger.LogWarning("Kanban task {TaskId} was not found for deletion", id);
                throw new NotFoundException($"Kanban task with ID {id} was not found.");
            }

            logger.LogInformation("Kanban task {TaskId} deleted successfully", id);

            return NoContent();
        }

        #endregion

        #region Attachments & Activities

        /// <summary>
        /// Retrieves all file attachments for a specific Kanban task.
        /// </summary>
        /// <param name="id">The ID of the task whose attachments are to be retrieved.</param>
        /// <returns>A list of attachments associated with the specified task.</returns>
        /// <response code="200">Returns the list of attachments for the task.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{id}/attachments")]
        [ProducesResponseType(typeof(IEnumerable<TaskAttachmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TaskAttachmentResponse>>> GetTaskAttachments(int id)
        {
            var attachments = await kanbanTaskRepository.GetTaskAttachmentsAsync(id);
            return Ok(attachments);
        }

        /// <summary>
        /// Retrieves the full activity history for a specific Kanban task.
        /// </summary>
        /// <param name="id">The ID of the task whose activity log is to be retrieved.</param>
        /// <returns>A list of activity records for the specified task.</returns>
        /// <response code="200">Returns the activity log for the task.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{id}/activities")]
        [ProducesResponseType(typeof(IEnumerable<TaskActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TaskActivityResponse>>> GetTaskActivities(int id)
        {
            var activities = await kanbanTaskRepository.GetTaskActivityResponsesAsync(id);
            return Ok(activities);
        }

        #endregion

        #region PDF Export

        /// <summary>
        /// Generates and downloads a PDF audit trail report for a specific Kanban task.
        /// The report includes full task details and a chronological activity history.
        /// </summary>
        /// <param name="id">The ID of the task to export.</param>
        /// <returns>A PDF file containing the task's audit trail.</returns>
        /// <response code="200">Returns the generated PDF file.</response>
        /// <response code="404">Task with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{id}/export-audit-pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportAuditTrailPdf(int id)
        {
            var task = await kanbanTaskRepository.GetTaskByTaskIdAsync(id);
            if (task is null)
            {
                logger.LogWarning("Kanban task {TaskId} was not found for PDF export", id);
                throw new NotFoundException($"Kanban task with ID {id} was not found.");
            }

            var activities = await kanbanTaskRepository.GetTaskActivityResponsesAsync(id);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = new AuditTrailPdfDocument(task, activities);
            var pdfBytes = document.GeneratePdf();

            logger.LogInformation("Audit trail PDF generated for Kanban task {TaskId} ({Bytes} bytes)", id, pdfBytes.Length);

            return File(pdfBytes, "application/pdf", $"audit-trail-TASK-{id}.pdf");
        }

        #endregion
    }
}