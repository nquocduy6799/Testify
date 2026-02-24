using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly ITestCaseRepository _testCaseRepository;

        public TestCasesController(ITestCaseRepository testCaseRepository)
        {
            _testCaseRepository = testCaseRepository;
        }

        // GET: api/TestCases/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCaseResponse>> GetById(int id)
        {
            var testCase = await _testCaseRepository.GetTestCaseByIdAsync(id);
            if (testCase is null) return NotFound();
            return Ok(testCase);
        }

        // POST: api/TestSuites/{suiteId}/testcases
        [HttpPost("~/api/testsuites/{suiteId}/testcases")]
        public async Task<ActionResult<TestCaseResponse>> Create(int suiteId, [FromBody] CreateTestCaseRequest request)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";
            var result = await _testCaseRepository.CreateTestCaseAsync(suiteId, request, userName);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/TestCases/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTestCaseRequest request)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "system";
            var success = await _testCaseRepository.UpdateTestCaseAsync(id, request, userName);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE: api/TestCases/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _testCaseRepository.DeleteTestCaseAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
