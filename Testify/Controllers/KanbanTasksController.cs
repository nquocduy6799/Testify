using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;

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
            var userName = User.Identity?.Name ?? "System";
            var updated = await _kanbanTaskRepository.UpdateTaskAsync(id, request, userName);

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "System";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var task = await _kanbanTaskRepository.CreateTaskAsync(request, userId, userName);

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
    }
}