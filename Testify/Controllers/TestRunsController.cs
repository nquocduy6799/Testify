using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.Enums;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestRunsController : ControllerBase
    {
        private readonly ITestRunRepository _testRunRepository;
        private readonly ILogger<TestRunsController> _logger;

        public TestRunsController(
            ITestRunRepository testRunRepository,
            ILogger<TestRunsController> logger)
        {
            _testRunRepository = testRunRepository;
            _logger = logger;
        }

        #region Test Run CRUD Operations

        /// <summary>
        /// Get a test run by ID
        /// </summary>
        /// <param name="id">Test run ID</param>
        /// <param name="includeSteps">Include test run steps in response</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TestRunResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestRunResponse>> GetById(
            int id,
            [FromQuery] bool includeSteps = false)
        {
            try
            {
                var testRun = await _testRunRepository.GetByIdAsync(id, includeSteps);

                if (testRun == null)
                    return NotFound($"Test run with ID {id} not found");

                return Ok(testRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test run {TestRunId}", id);
                return StatusCode(500, "An error occurred while retrieving the test run");
            }
        }

        /// <summary>
        /// Get detailed test run with all steps and attachments
        /// </summary>
        [HttpGet("{id}/detailed")]
        [ProducesResponseType(typeof(TestRunDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestRunDetailResponse>> GetDetailed(int id)
        {
            try
            {
                var testRun = await _testRunRepository.GetDetailedByIdAsync(id);

                if (testRun == null)
                    return NotFound($"Test run with ID {id} not found");

                return Ok(testRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed test run {TestRunId}", id);
                return StatusCode(500, "An error occurred while retrieving the test run");
            }
        }

        /// <summary>
        /// Get all test runs for a test plan
        /// </summary>
        [HttpGet("testplan/{testPlanId}")]
        [ProducesResponseType(typeof(IEnumerable<TestRunResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TestRunResponse>>> GetByTestPlanId(int testPlanId)
        {
            try
            {
                var testRuns = await _testRunRepository.GetByTestPlanIdAsync(testPlanId);
                return Ok(testRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test runs for plan {TestPlanId}", testPlanId);
                return StatusCode(500, "An error occurred while retrieving test runs");
            }
        }

        /// <summary>
        /// Get test runs by status for a test plan
        /// </summary>
        [HttpGet("testplan/{testPlanId}/status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<TestRunResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TestRunResponse>>> GetByStatus(
            int testPlanId,
            TestRunStatus status)
        {
            try
            {
                var testRuns = await _testRunRepository.GetByStatusAsync(testPlanId, status);
                return Ok(testRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test runs by status for plan {TestPlanId}", testPlanId);
                return StatusCode(500, "An error occurred while retrieving test runs");
            }
        }

        /// <summary>
        /// Get test runs executed by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<TestRunResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TestRunResponse>>> GetByExecutedBy(string userId)
        {
            try
            {
                var testRuns = await _testRunRepository.GetByExecutedByAsync(userId);
                return Ok(testRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test runs for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving test runs");
            }
        }

        /// <summary>
        /// Create a new test run
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TestRunResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TestRunResponse>> Create([FromBody] CreateTestRunRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var testRun = await _testRunRepository.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = testRun.Id },
                    testRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test run");
                return StatusCode(500, "An error occurred while creating the test run");
            }
        }

        /// <summary>
        /// Update an existing test run
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TestRunResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TestRunResponse>> Update(
            int id,
            [FromBody] UpdateTestRunRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var testRun = await _testRunRepository.UpdateAsync(id, request);

                return Ok(testRun);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Test run {TestRunId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating test run {TestRunId}", id);
                return StatusCode(500, "An error occurred while updating the test run");
            }
        }

        /// <summary>
        /// Delete a test run
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _testRunRepository.DeleteAsync(id);

                if (!result)
                    return NotFound($"Test run with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting test run {TestRunId}", id);
                return StatusCode(500, "An error occurred while deleting the test run");
            }
        }

        #endregion

        #region Start Test Plan Execution

        /// <summary>
        /// Start test plan execution 
        /// Creates test runs and snapshots test steps for all test cases in the selected suites
        /// </summary>
        [HttpPost("start-execution")]
        [ProducesResponseType(typeof(StartExecutionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StartExecutionResponse>> StartExecution(
            [FromBody] StartExecutionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!request.TestSuiteIds.Any())
                    return BadRequest("At least one test suite must be selected");

                _logger.LogInformation(
                    "Starting execution for test plan {TestPlanId} with {SuiteCount} suites",
                    request.TestPlanId,
                    request.TestSuiteIds.Count);

                var totalCreated = await _testRunRepository.StartTestPlanExecutionAsync(
                    request.TestPlanId,
                    request.TestSuiteIds);

                return Ok(new StartExecutionResponse
                {
                    TestPlanId = request.TestPlanId,
                    TotalTestRunsCreated = totalCreated,
                    Message = $"Successfully created {totalCreated} test runs and started execution"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Test plan {TestPlanId} not found", request.TestPlanId);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for test plan {TestPlanId}", request.TestPlanId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting execution for test plan {TestPlanId}", request.TestPlanId);
                return StatusCode(500, "An error occurred while starting test plan execution");
            }
        }

        /// <summary>
        /// Bulk create test runs without starting execution
        /// </summary>
        [HttpPost("bulk-create")]
        [ProducesResponseType(typeof(BulkCreateTestRunsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BulkCreateTestRunsResponse>> BulkCreate(
            [FromBody] BulkCreateTestRunsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!request.TestCaseIds.Any())
                    return BadRequest("At least one test case must be provided");

                var response = await _testRunRepository.BulkCreateTestRunsAsync(
                    request.TestPlanId,
                    request.TestCaseIds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating test runs");
                return StatusCode(500, "An error occurred while creating test runs");
            }
        }

        #endregion

        #region Test Run Step Operations

        /// <summary>
        /// Get all steps for a test run
        /// </summary>
        [HttpGet("{testRunId}/steps")]
        [ProducesResponseType(typeof(IEnumerable<TestRunStepResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TestRunStepResponse>>> GetSteps(int testRunId)
        {
            try
            {
                var steps = await _testRunRepository.GetStepsByTestRunIdAsync(testRunId);
                return Ok(steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting steps for test run {TestRunId}", testRunId);
                return StatusCode(500, "An error occurred while retrieving test run steps");
            }
        }

        /// <summary>
        /// Get a specific test run step
        /// </summary>
        [HttpGet("steps/{stepId}")]
        [ProducesResponseType(typeof(TestRunStepResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TestRunStepResponse>> GetStep(int stepId)
        {
            try
            {
                var step = await _testRunRepository.GetStepByIdAsync(stepId);

                if (step == null)
                    return NotFound($"Test run step with ID {stepId} not found");

                return Ok(step);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test run step {StepId}", stepId);
                return StatusCode(500, "An error occurred while retrieving the test run step");
            }
        }

        /// <summary>
        /// Update a test run step (log test results)
        /// </summary>
        [HttpPut("steps/{stepId}")]
        [ProducesResponseType(typeof(TestRunStepResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TestRunStepResponse>> UpdateStep(
            int stepId,
            [FromBody] UpdateTestRunStepRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var step = await _testRunRepository.UpdateStepAsync(stepId, request);

                return Ok(step);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Test run step {StepId} not found", stepId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating test run step {StepId}", stepId);
                return StatusCode(500, "An error occurred while updating the test run step");
            }
        }

        /// <summary>
        /// Batch update multiple test run steps
        /// </summary>
        [HttpPut("steps/batch")]
        [ProducesResponseType(typeof(IEnumerable<TestRunStepResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TestRunStepResponse>>> BatchUpdateSteps(
            [FromBody] Dictionary<int, UpdateTestRunStepRequest> updates)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!updates.Any())
                    return BadRequest("At least one update must be provided");

                var steps = await _testRunRepository.BatchUpdateStepsAsync(updates);

                return Ok(steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch updating test run steps");
                return StatusCode(500, "An error occurred while updating test run steps");
            }
        }

        #endregion

        #region Statistics & Reporting

        /// <summary>
        /// Get test run statistics for a test plan
        /// </summary>
        [HttpGet("testplan/{testPlanId}/statistics")]
        [ProducesResponseType(typeof(TestRunStatistics), StatusCodes.Status200OK)]
        public async Task<ActionResult<TestRunStatistics>> GetStatistics(int testPlanId)
        {
            try
            {
                var stats = await _testRunRepository.GetStatisticsByTestPlanIdAsync(testPlanId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for test plan {TestPlanId}", testPlanId);
                return StatusCode(500, "An error occurred while retrieving statistics");
            }
        }

        /// <summary>
        /// Check if a test plan has any test runs
        /// </summary>
        [HttpGet("testplan/{testPlanId}/has-runs")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasTestRuns(int testPlanId)
        {
            try
            {
                var hasRuns = await _testRunRepository.HasTestRunsAsync(testPlanId);
                return Ok(hasRuns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking test runs for plan {TestPlanId}", testPlanId);
                return StatusCode(500, "An error occurred while checking test runs");
            }
        }

        #endregion
    }
}