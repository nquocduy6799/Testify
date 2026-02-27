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

        // POST: api/TestSuites
        [HttpPost]
        public async Task<ActionResult<TestSuiteResponse>> Create([FromBody] CreateTestSuiteRequest request)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";

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
