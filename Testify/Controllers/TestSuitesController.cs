using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSuitesController : ControllerBase
    {
        private readonly ITestSuiteRepository _testSuiteRepository;

        public TestSuitesController(ITestSuiteRepository testSuiteRepository)
        {
            _testSuiteRepository = testSuiteRepository;
        }

        // GET: api/TestSuites/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TestSuiteResponse>>> GetByProject(int projectId)
        {
            var suites = await _testSuiteRepository.GetTestSuitesByProjectIdAsync(projectId);
            return Ok(suites);
        }

        // GET: api/TestSuites/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSuiteResponse>> GetById(int id)
        {
            var suite = await _testSuiteRepository.GetTestSuiteByIdAsync(id);
            if (suite is null) return NotFound();
            return Ok(suite);
        }

        // GET: api/TestSuites/check-name?projectId=1&name=xxx&excludeId=2
        [HttpGet("check-name")]
        public async Task<ActionResult<bool>> CheckNameExists([FromQuery] int projectId, [FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            var exists = await _testSuiteRepository.IsSuiteNameExistsAsync(projectId, name, excludeId);
            return Ok(exists);
        }

        // GET: api/TestSuites/suggest-name?projectId=1&baseName=xxx
        [HttpGet("suggest-name")]
        public async Task<ActionResult<string>> SuggestUniqueName([FromQuery] int projectId, [FromQuery] string baseName)
        {
            var uniqueName = await _testSuiteRepository.GenerateUniqueSuiteNameAsync(projectId, baseName);
            return Ok(uniqueName);
        }

        // POST: api/TestSuites
        [HttpPost]
        public async Task<ActionResult<TestSuiteResponse>> Create([FromBody] CreateTestSuiteRequest request)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";

            // Determine the effective name
            string effectiveName = request.Name;

            // Check for duplicate name in the project (name is always provided by UI now)
            if (!string.IsNullOrWhiteSpace(effectiveName))
            {
                var nameExists = await _testSuiteRepository.IsSuiteNameExistsAsync(request.ProjectId, effectiveName);
                if (nameExists)
                    return Conflict(new { message = $"A test suite named \"{effectiveName}\" already exists in this project." });
            }

            TestSuiteResponse result;

            if (request.SourceTemplateId.HasValue)
            {
                var fromTemplate = await _testSuiteRepository.CreateTestSuiteFromTemplateAsync(request, userName);
                if (fromTemplate is null)
                    return BadRequest("Source template not found.");
                result = fromTemplate;
            }
            else
            {
                result = await _testSuiteRepository.CreateTestSuiteAsync(request, userName);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/TestSuites/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTestSuiteRequest request)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";

            // Get the existing suite to know its ProjectId
            var existing = await _testSuiteRepository.GetTestSuiteByIdAsync(id);
            if (existing is null) return NotFound();

            // Check for duplicate name (excluding current suite)
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameExists = await _testSuiteRepository.IsSuiteNameExistsAsync(existing.ProjectId, request.Name, id);
                if (nameExists)
                    return Conflict(new { message = $"A test suite named \"{request.Name}\" already exists in this project." });
            }

            var success = await _testSuiteRepository.UpdateTestSuiteAsync(id, request, userName);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE: api/TestSuites/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";
            var success = await _testSuiteRepository.DeleteTestSuiteAsync(id, userName);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
