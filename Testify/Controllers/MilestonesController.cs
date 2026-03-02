using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Milestones;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MilestonesController : ControllerBase
    {
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public MilestonesController(IMilestoneRepository milestoneRepository, IHubContext<NotificationHub> hubContext)
        {
            _milestoneRepository = milestoneRepository;
            _hubContext = hubContext;
        }

        // GET: api/Milestones/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<MilestoneResponse>>> GetMilestonesByProject(int projectId)
        {
            var milestones = await _milestoneRepository.GetMilestonesByProjectIdAsync(projectId);
            return Ok(milestones);
        }

        // GET: api/Milestones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MilestoneResponse>> GetMilestone(int id)
        {
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            return Ok(milestone);
        }

        // POST: api/Milestones
        [HttpPost]
        public async Task<ActionResult<MilestoneResponse>> CreateMilestone(CreateMilestoneRequest request)
        {
            var userName = User.Identity?.Name ?? "System";

            if (!request.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

            if (!request.IsValidStatusForCreation())
            {
                ModelState.AddModelError("Status", "New milestones can only be created with Active or Planned status.");
                return BadRequest(ModelState);
            }

            var response = await _milestoneRepository.CreateMilestoneAsync(request, userName);

            // Broadcast milestone created via SignalR to project group
            await _hubContext.Clients.Group($"project_{response.ProjectId}").SendAsync("MilestoneCreated", response.ProjectId, response);
            Console.WriteLine($"[MilestonesController] Broadcast MilestoneCreated - Project {response.ProjectId}");

            return CreatedAtAction("GetMilestone", new { id = response.Id }, response);
        }

        // PUT: api/Milestones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(int id, UpdateMilestoneRequest request)
        {
            var userName = User.Identity?.Name ?? "System";

            if (id != request.Id)
            {
                return BadRequest();
            }

            if (!request.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

            var updated = await _milestoneRepository.UpdateMilestoneAsync(id, request, userName);
            if (!updated)
            {
                return NotFound();
            }

            // Fetch updated milestone to broadcast
            var updatedMilestone = await _milestoneRepository.GetMilestoneByIdAsync(id);
            if (updatedMilestone != null)
            {
                await _hubContext.Clients.Group($"project_{updatedMilestone.ProjectId}").SendAsync("MilestoneUpdated", updatedMilestone.ProjectId, updatedMilestone);
                Console.WriteLine($"[MilestonesController] Broadcast MilestoneUpdated - Project {updatedMilestone.ProjectId}");
            }

            return NoContent();
        }

        // DELETE: api/Milestones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMilestone(int id)
        {
            var userName = User.Identity?.Name ?? "System";

            // Get milestone info before deleting for SignalR broadcast
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(id);
            if (milestone == null)
            {
                return NotFound();
            }

            var projectId = milestone.ProjectId;

            var deleted = await _milestoneRepository.DeleteMilestoneAsync(id, userName);
            if (!deleted)
            {
                return NotFound();
            }

            // Broadcast milestone deleted via SignalR to project group
            await _hubContext.Clients.Group($"project_{projectId}").SendAsync("MilestoneDeleted", projectId, id);
            Console.WriteLine($"[MilestonesController] Broadcast MilestoneDeleted - Project {projectId}, Milestone {id}");

            return NoContent();
        }
    }
}
