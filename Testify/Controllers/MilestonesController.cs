using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Testify.Data;
using Testify.Entities;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.Milestones;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilestonesController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IKanbanTaskRepository _kanbanTaskRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public MilestonesController(ApplicationDbContext context, IKanbanTaskRepository kanbanTaskRepository, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _kanbanTaskRepository = kanbanTaskRepository;
            _hubContext = hubContext;
        }

        // GET: api/Milestones/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<MilestoneResponse>>> GetMilestonesByProject(int projectId)
        {
            var milestones = await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.StartDate)
                .ToListAsync();

            var response = new List<MilestoneResponse>();

            foreach (var milestone in milestones)
            {
                var tasks = await _kanbanTaskRepository.GetTasksByMilestoneIdAsync(milestone.Id);

                response.Add(new MilestoneResponse
                {
                    Id = milestone.Id,
                    ProjectId = milestone.ProjectId,
                    Name = milestone.Name,
                    Description = milestone.Description,
                    StartDate = milestone.StartDate,
                    EndDate = milestone.EndDate,
                    Status = milestone.Status,
                    Tasks = tasks.ToList()
                });
            }

            return response;
        }

        // GET: api/Milestones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MilestoneResponse>> GetMilestone(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            return new MilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                Description = milestone.Description,
                StartDate = milestone.StartDate,
                EndDate = milestone.EndDate,
                Status = milestone.Status
            };
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

            var milestone = new Milestone
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = MilestoneStatus.Active
            };

            milestone.MarkAsCreated(userName);

            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            var response = new MilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                Description = milestone.Description,
                StartDate = milestone.StartDate,
                EndDate = milestone.EndDate,
                Status = milestone.Status
            };

            // Broadcast milestone created via SignalR
            await _hubContext.Clients.All.SendAsync("MilestoneCreated", milestone.ProjectId, response);
            Console.WriteLine($"[MilestonesController] Broadcast MilestoneCreated - Project {milestone.ProjectId}");

            return CreatedAtAction("GetMilestone", new { id = milestone.Id }, response);
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

            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone == null)
            {
                return NotFound();
            }

            milestone.Name = request.Name;
            milestone.Description = request.Description;
            milestone.StartDate = request.StartDate;
            milestone.EndDate = request.EndDate;
            milestone.Status = request.Status;

            milestone.MarkAsUpdated(userName);

            _context.Entry(milestone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                
                // Broadcast milestone updated via SignalR
                var updatedResponse = new MilestoneResponse
                {
                    Id = milestone.Id,
                    ProjectId = milestone.ProjectId,
                    Name = milestone.Name,
                    Description = milestone.Description,
                    StartDate = milestone.StartDate,
                    EndDate = milestone.EndDate,
                    Status = milestone.Status
                };
                
                await _hubContext.Clients.All.SendAsync("MilestoneUpdated", milestone.ProjectId, updatedResponse);
                Console.WriteLine($"[MilestonesController] Broadcast MilestoneUpdated - Project {milestone.ProjectId}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MilestoneExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Milestones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMilestone(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone == null)
            {
                return NotFound();
            }

            var projectId = milestone.ProjectId;
            
            _context.Milestones.Remove(milestone);
            await _context.SaveChangesAsync();

            // Broadcast milestone deleted via SignalR
            await _hubContext.Clients.All.SendAsync("MilestoneDeleted", projectId, id);
            Console.WriteLine($"[MilestonesController] Broadcast MilestoneDeleted - Project {projectId}, Milestone {id}");

            return NoContent();
        }

        private bool MilestoneExists(int id)
        {
            return _context.Milestones.Any(e => e.Id == id);
        }
    }
}
