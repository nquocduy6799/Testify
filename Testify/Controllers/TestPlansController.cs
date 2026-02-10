using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestPlansController : ControllerBase
    {
        private readonly ITestPlanRepository _testPlanRepository;
        private readonly ITestPlanSuiteRepository _testPlanSuiteRepository;

        public TestPlansController(ITestPlanRepository testPlanRepository, ITestPlanSuiteRepository testPlanSuiteRepository)
        {
            _testPlanRepository = testPlanRepository;
            _testPlanSuiteRepository = testPlanSuiteRepository;
        }

        // GET: api/TestPlans/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TestPlanResponse>>> GetTestPlansByProject(int projectId)
        {
            var testPlans = await _testPlanRepository.GetAllTestPlansAsync(projectId);
            return Ok(testPlans);
        }

        // GET: api/TestPlans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestPlanResponse>> GetTestPlan(int id)
        {
            var testPlan = await _testPlanRepository.GetTestPlanByIdAsync(id);

            if (testPlan == null)
            {
                return NotFound();
            }

            return Ok(testPlan);
        }

        // POST: api/TestPlans
        [HttpPost]
        public async Task<ActionResult<TestPlanResponse>> PostTestPlan(CreateTestPlanRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var userName = User.Identity?.Name ?? "System";

            var testPlan = await _testPlanRepository.CreateTestPlanAsync(request, userName, userId);

            return CreatedAtAction(nameof(GetTestPlan), new { id = testPlan.Id }, testPlan);
        }

        // PUT: api/TestPlans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestPlan(int id, UpdateTestPlanRequest request)
        {
            var userName = User.Identity?.Name ?? "System";
            var updated = await _testPlanRepository.UpdateTestPlanAsync(id, request, userName);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/TestPlans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestPlan(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await _testPlanRepository.DeleteTestPlanAsync(id, userName);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        //Get all test suites by project id
        [HttpGet("project/{projectId}/testsuites")]
        public async Task<ActionResult<IEnumerable<TestSuiteResponse>>> GetTestSuitesByProjectId(int projectId)
        {
            var testSuites = await _testPlanSuiteRepository.GetAllTestSuitesByProjectIdAsync(projectId);
            return Ok(testSuites);
        }
    }
}