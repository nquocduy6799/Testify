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
        public async Task<ActionResult<IEnumerable<MilestoneDTO>>> GetMilestonesByProject(int projectId)
        {
            var milestones = await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.StartDate)
                .ToListAsync();

            var dtos = milestones.Select(m => new MilestoneDTO
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                Name = m.Name,
                Description = m.Description,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Status = m.Status
            }).ToList();

            return dtos;
        }

        // GET: api/Milestones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MilestoneDTO>> GetMilestone(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            return new MilestoneDTO
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
        public async Task<ActionResult<MilestoneDTO>> CreateMilestone(CreateMilestoneDTO dto)
        {
            if (!dto.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

            var milestone = new Milestone
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Planned"
            };

            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            var resultDTO = new MilestoneDTO
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                Description = milestone.Description,
                StartDate = milestone.StartDate,
                EndDate = milestone.EndDate,
                Status = milestone.Status
            };

            return CreatedAtAction("GetMilestone", new { id = milestone.Id }, resultDTO);
        }

        // PUT: api/Milestones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(int id, UpdateMilestoneDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if (!dto.IsValidDateRange())
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than or equal to Start Date.");
                return BadRequest(ModelState);
            }

            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone == null)
            {
                return NotFound();
            }

            milestone.Name = dto.Name;
            milestone.Description = dto.Description;
            milestone.StartDate = dto.StartDate;
            milestone.EndDate = dto.EndDate;
            milestone.Status = dto.Status;

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
