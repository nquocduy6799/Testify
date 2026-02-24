using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.TaskActivity;
using Testify.Shared.DTOs.TaskAttachments;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KanbanTasksController : ControllerBase
    {
        private readonly IKanbanTaskRepository _kanbanTaskRepository;

        public KanbanTasksController(IKanbanTaskRepository kanbanTaskRepository)
        {
            _kanbanTaskRepository = kanbanTaskRepository;
        }

        // GET: api/KanbanTasks/milestone/5
        [HttpGet("milestone/{milestoneId}")]
        public async Task<ActionResult<IEnumerable<KanbanTaskResponse>>> GetTasksByMilestone(int milestoneId)
        {
            var tasks = await _kanbanTaskRepository.GetTasksByMilestoneIdAsync(milestoneId);
            return Ok(tasks);
        }

        // GET: api/KanbanTasks/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<KanbanTaskResponse>>> GetTasksByProject(int projectId)
        {
            var tasks = await _kanbanTaskRepository.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
        }

        // GET: api/KanbanTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KanbanTaskResponse>> GetTask(int id)
        {
            var task = await _kanbanTaskRepository.GetTaskByTaskIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        // PUT: api/KanbanTasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, UpdateKanbanTaskRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.Identity?.Name ?? "System";
            var updated = await _kanbanTaskRepository.UpdateTaskAsync(id, request, userName, userId);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }


        // POST: api/KanbanTasks
        [HttpPost]
        public async Task<ActionResult<KanbanTaskResponse>> PostTask(CreateKanbanTaskRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.Identity?.Name ?? "System";

            var task = await _kanbanTaskRepository.CreateTaskAsync(request, userName, userId);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // DELETE: api/KanbanTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await _kanbanTaskRepository.DeleteTaskAsync(id, userName);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<TaskAttachmentResponse>>> GetTaskAttachments(int id)
        {
            var attachments = await _kanbanTaskRepository.GetTaskAttachmentsAsync(id);

            if (attachments == null)
            {
                return NotFound();
            }

            return Ok(attachments);
        }


        [HttpGet("{id}/activities")]
        public async Task<ActionResult<IEnumerable<TaskActivityResponse>>> GetTaskActivities(int id)
        {
            var activities = await _kanbanTaskRepository.GetTaskActivityResponsesAsync(id);

            if (activities == null)
            {
                return NotFound();
            }

            return Ok(activities);
        }
    }
}