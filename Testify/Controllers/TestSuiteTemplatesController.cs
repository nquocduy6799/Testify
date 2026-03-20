using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestTemplates;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestSuiteTemplatesController : ControllerBase
    {
        private readonly ITestSuiteTemplateRepository _testSuiteTemplateRepository;
        private readonly ICurrentUserRepository _currentUserRepository;

        public TestSuiteTemplatesController(ITestSuiteTemplateRepository testSuiteTemplateRepository, ICurrentUserRepository currentUserRepository)
        {
            _testSuiteTemplateRepository = testSuiteTemplateRepository;
            _currentUserRepository = currentUserRepository;
        }

        // GET: api/TestSuiteTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteTemplateResponse>>> GetTestSuiteTemplates()
        {
            var templates = await _testSuiteTemplateRepository.GetAllTestSuiteTemplatesAsync();
            return Ok(templates);
        }

        // GET: api/TestSuiteTemplates/cloneable
        [HttpGet("cloneable")]
        public async Task<ActionResult<IEnumerable<TestSuiteTemplateResponse>>> GetCloneableTemplates()
        {
            var userId = _currentUserRepository.UserId ?? string.Empty;
            var templates = await _testSuiteTemplateRepository.GetCloneableTemplatesAsync(userId);
            return Ok(templates);
        }

        // GET: api/TestSuiteTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSuiteTemplateResponse>> GetTestSuiteTemplate(int id)
        {
            var template = await _testSuiteTemplateRepository.GetTestSuiteTemplateByIdAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return Ok(template);
        }

        // POST: api/TestSuiteTemplates
        [HttpPost]
        public async Task<ActionResult<TestSuiteTemplateResponse>> PostTestSuiteTemplate(CreateTestSuiteTemplateRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var userName = _currentUserRepository.UserName ?? "System";
            var template = await _testSuiteTemplateRepository.CreateTestSuiteTemplateAsync(request, userName, userId);

            return CreatedAtAction(nameof(GetTestSuiteTemplate), new { id = template.Id }, template);
        }

        // PUT: api/TestSuiteTemplates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestSuiteTemplate(int id, UpdateTestSuiteTemplateRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var userName = _currentUserRepository.UserName ?? "System";
            var updated = await _testSuiteTemplateRepository.UpdateTestSuiteTemplateAsync(id, request, userName);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/TestSuiteTemplates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestSuiteTemplate(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await _testSuiteTemplateRepository.DeleteTestSuiteTemplateAsync(id, userName);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/TestSuiteTemplates/{id}/view
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            await _testSuiteTemplateRepository.IncrementViewCountAsync(id);
            return Ok();
        }

        // POST: api/TestSuiteTemplates/{id}/clone
        [HttpPost("{id}/clone")]
        public async Task<IActionResult> IncrementCloneCount(int id)
        {
            var ok = await _testSuiteTemplateRepository.IncrementCloneCountAsync(id);
            if (!ok) return NotFound();
            return Ok();
        }

        // POST: api/TestSuiteTemplates/bulk-delete
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkIdsRequest request)
        {
            var userName = _currentUserRepository.UserName ?? "System";
            var (deleted, failed) = await _testSuiteTemplateRepository.BulkDeleteAsync(request.TemplateIds, userName);
            return Ok(new { Deleted = deleted, Failed = failed });
        }

        // POST: api/TestSuiteTemplates/bulk-move
        [HttpPost("bulk-move")]
        public async Task<IActionResult> BulkMove([FromBody] BulkMoveRequest request)
        {
            var userName = _currentUserRepository.UserName ?? "System";
            var (moved, failed) = await _testSuiteTemplateRepository.BulkMoveAsync(request.TemplateIds, request.TargetFolderId, userName);
            return Ok(new { Moved = moved, Failed = failed });
        }
    }

    public class BulkIdsRequest
    {
        public List<int> TemplateIds { get; set; } = new();
    }

    public class BulkMoveRequest
    {
        public List<int> TemplateIds { get; set; } = new();
        public int? TargetFolderId { get; set; }
    }
}