using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.DTOs.Milestones;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilestonesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Milestones/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<MilestoneResponse>>> GetMilestonesByProject(int projectId)
        {
            var milestones = await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.StartDate)
                .ToListAsync();

            var response = milestones.Select(m => new MilestoneResponse
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                Name = m.Name,
                Description = m.Description,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Status = m.Status,
            }).ToList();

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
                Status = "Planned"
            };

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

            return CreatedAtAction("GetMilestone", new { id = milestone.Id }, response);
        }

        // PUT: api/Milestones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(int id, UpdateMilestoneRequest request)
        {
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

            _context.Entry(milestone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            _context.Milestones.Remove(milestone);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MilestoneExists(int id)
        {
            return _context.Milestones.Any(e => e.Id == id);
        }
    }
}
