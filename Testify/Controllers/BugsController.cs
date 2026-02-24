using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.Bugs;
using Testify.Shared.Enums;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BugsController : ControllerBase
    {
        private readonly IBugRepository _bugRepository;

        public BugsController(IBugRepository bugRepository)
        {
            _bugRepository = bugRepository;
        }

        // POST: api/Bugs/fromtestrun
        [HttpPost("fromtestrun")]
        public async Task<ActionResult<BugResponse>> CreateBugFromTestRun(CreateBugFromTestRunRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.Identity?.Name ?? "System";

            try
            {
                var bug = await _bugRepository.CreateBugFromTestRunAsync(request, userName, userId);
                return CreatedAtAction(nameof(GetBugById), new { id = bug.Id }, bug);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/Bugs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BugResponse>> GetBugById(int id)
        {
            var bug = await _bugRepository.GetBugByIdAsync(id);

            if (bug == null)
            {
                return NotFound(new { message = $"Bug with ID {id} not found." });
            }

            return Ok(bug);
        }

        // GET: api/Bugs/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugsByProject(int projectId)
        {
            var bugs = await _bugRepository.GetBugsByProjectIdAsync(projectId);
            return Ok(bugs);
        }

        // GET: api/Bugs/milestone/5
        [HttpGet("milestone/{milestoneId}")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugsByMilestone(int milestoneId)
        {
            var bugs = await _bugRepository.GetBugsByMilestoneIdAsync(milestoneId);
            return Ok(bugs);
        }

        // GET: api/Bugs/testrun/5
        [HttpGet("testrun/{testRunId}")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugsByTestRun(int testRunId)
        {
            var bugs = await _bugRepository.GetBugsByTestRunIdAsync(testRunId);
            return Ok(bugs);
        }

        // GET: api/Bugs/assignee/{assigneeId}
        [HttpGet("assignee/{assigneeId}")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugsByAssignee(string assigneeId)
        {
            var bugs = await _bugRepository.GetBugsByAssigneeIdAsync(assigneeId);
            return Ok(bugs);
        }

        // GET: api/Bugs/reporter/{reporterId}
        [HttpGet("reporter/{reporterId}")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugsByReporter(string reporterId)
        {
            var bugs = await _bugRepository.GetBugsByReporterIdAsync(reporterId);
            return Ok(bugs);
        }

        // GET: api/Bugs/summary/project/5
        [HttpGet("summary/project/{projectId}")]
        public async Task<ActionResult<BugSummary>> GetBugSummaryByProject(int projectId)
        {
            var summary = await _bugRepository.GetBugSummaryByProjectIdAsync(projectId);
            return Ok(summary);
        }

        // GET: api/Bugs/summary/milestone/5
        [HttpGet("summary/milestone/{milestoneId}")]
        public async Task<ActionResult<BugSummary>> GetBugSummaryByMilestone(int milestoneId)
        {
            var summary = await _bugRepository.GetBugSummaryByMilestoneIdAsync(milestoneId);
            return Ok(summary);
        }

        // POST: api/Bugs/linkrunstep
        [HttpPost("linkrunstep")]
        public async Task<IActionResult> LinkRunStepToBug(LinkRunStepToBugRequest request)
        {
            var userName = User.Identity?.Name ?? "System";

            var success = await _bugRepository.LinkRunStepToBugAsync(request, userName);

            if (!success)
            {
                return BadRequest(new { message = "Failed to link test run step to bug. Bug or run step not found, or link already exists." });
            }

            return Ok(new { message = "Test run step successfully linked to bug." });
        }

        // DELETE: api/Bugs/5/unlinkrunstep/10
        [HttpDelete("{bugId}/unlinkrunstep/{runStepId}")]
        public async Task<IActionResult> UnlinkRunStepFromBug(int bugId, int runStepId)
        {
            var success = await _bugRepository.UnlinkRunStepFromBugAsync(bugId, runStepId);

            if (!success)
            {
                return NotFound(new { message = "Link between bug and run step not found." });
            }

            return Ok(new { message = "Test run step successfully unlinked from bug." });
        }

        // PATCH: api/Bugs/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBugStatus(int id, [FromBody] UpdateBugStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.Identity?.Name ?? "System";

            var success = await _bugRepository.UpdateBugStatusAsync(id, request.Status, userName, userId);

            if (!success)
            {
                return NotFound(new { message = $"Bug with ID {id} not found." });
            }

            return Ok(new { message = "Bug status updated successfully." });
        }

        // GET: api/Bugs/my
        // Get bugs assigned to the current user
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetMyBugs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var bugs = await _bugRepository.GetBugsByAssigneeIdAsync(userId);
            return Ok(bugs);
        }

        // GET: api/Bugs/myreported
        // Get bugs reported by the current user
        [HttpGet("myreported")]
        public async Task<ActionResult<IEnumerable<BugResponse>>> GetMyReportedBugs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var bugs = await _bugRepository.GetBugsByReporterIdAsync(userId);
            return Ok(bugs);
        }
    }

    // Helper DTO for status update endpoint
    public class UpdateBugStatusRequest
    {
        public KanbanTaskStatus Status { get; set; }
    }
}