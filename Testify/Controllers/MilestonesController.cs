using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Shared.Entities;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilestonesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Milestones/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<Milestone>>> GetMilestonesByProject(int projectId)
        {
            return await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.StartDate)
                .ToListAsync();
        }

        // GET: api/Milestones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Milestone>> GetMilestone(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            return milestone;
        }

        // POST: api/Milestones
        [HttpPost]
        public async Task<ActionResult<Milestone>> CreateMilestone(Milestone milestone)
        {
            if (!milestone.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMilestone", new { id = milestone.Id }, milestone);
        }

        // PUT: api/Milestones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(int id, Milestone milestone)
        {
            if (id != milestone.Id)
            {
                return BadRequest();
            }

            if (!milestone.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

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
