using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;

namespace Testify.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing test plans and their associated test suites within a project.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestPlansController(
        ITestPlanRepository testPlanRepository,
        ITestPlanSuiteRepository testPlanSuiteRepository) : ControllerBase
    {
        #region Test Plans

        /// <summary>
        /// Retrieves all test plans belonging to a specific project.
        /// </summary>
        /// <param name="projectId">The ID of the project whose test plans are to be retrieved.</param>
        /// <returns>A list of test plans associated with the specified project.</returns>
        /// <response code="200">Returns the list of test plans for the project.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("project/{projectId}")]
        [ProducesResponseType(typeof(IEnumerable<TestPlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TestPlanResponse>>> GetTestPlansByProject(int projectId)
        {
            var testPlans = await testPlanRepository.GetAllTestPlansAsync(projectId);
            return Ok(testPlans);
        }

        /// <summary>
        /// Retrieves a specific test plan by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the test plan to retrieve.</param>
        /// <returns>The test plan with the specified ID.</returns>
        /// <response code="200">Returns the requested test plan.</response>
        /// <response code="404">Test plan with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TestPlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TestPlanResponse>> GetTestPlan(int id)
        {
            var testPlan = await testPlanRepository.GetTestPlanByIdAsync(id)
                ?? throw new NotFoundException($"Test plan with ID {id} was not found.");

            return Ok(testPlan);
        }

        /// <summary>
        /// Creates a new test plan.
        /// </summary>
        /// <param name="request">The data required to create the test plan.</param>
        /// <returns>The newly created test plan.</returns>
        /// <response code="201">Test plan created successfully. Returns the created test plan.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TestPlanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TestPlanResponse>> PostTestPlan(CreateTestPlanRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var userName = User.Identity?.Name ?? "System";

            var testPlan = await testPlanRepository.CreateTestPlanAsync(request, userName, userId);

            return CreatedAtAction(nameof(GetTestPlan), new { id = testPlan.Id }, testPlan);
        }

        /// <summary>
        /// Updates an existing test plan.
        /// </summary>
        /// <param name="id">The ID of the test plan to update.</param>
        /// <param name="request">The updated test plan data.</param>
        /// <returns>No content if the update is successful.</returns>
        /// <response code="204">Test plan updated successfully.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="404">Test plan with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutTestPlan(int id, UpdateTestPlanRequest request)
        {
            var userName = User.Identity?.Name ?? "System";
            var updated = await testPlanRepository.UpdateTestPlanAsync(id, request, userName);

            if (!updated)
                throw new NotFoundException($"Test plan with ID {id} was not found.");

            return NoContent();
        }

        /// <summary>
        /// Deletes a test plan by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the test plan to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        /// <response code="204">Test plan deleted successfully.</response>
        /// <response code="404">Test plan with the given ID was not found.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTestPlan(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await testPlanRepository.DeleteTestPlanAsync(id, userName);

            if (!deleted)
                throw new NotFoundException($"Test plan with ID {id} was not found.");

            return NoContent();
        }

        #endregion

        #region Test Suites

        /// <summary>
        /// Retrieves all test suites that belong to a specific project,
        /// regardless of their test plan assignment.
        /// </summary>
        /// <param name="projectId">The ID of the project whose test suites are to be retrieved.</param>
        /// <returns>A list of test suites associated with the specified project.</returns>
        /// <response code="200">Returns the list of test suites for the project.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet("project/{projectId}/testsuites")]
        [ProducesResponseType(typeof(IEnumerable<TestSuiteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TestSuiteResponse>>> GetTestSuitesByProjectId(int projectId)
        {
            var testSuites = await testPlanSuiteRepository.GetAllTestSuitesByProjectIdAsync(projectId);
            return Ok(testSuites);
        }

        #endregion
    }
}