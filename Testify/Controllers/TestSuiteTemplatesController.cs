using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestTemplates;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TestSuiteTemplatesController : ControllerBase
    {
        private readonly ITestSuiteTemplateRepository _testSuiteTemplateRepository;

        public TestSuiteTemplatesController(ITestSuiteTemplateRepository testSuiteTemplateRepository)
        {
            _testSuiteTemplateRepository = testSuiteTemplateRepository;
        }

        // GET: api/TestSuiteTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteTemplateResponse>>> GetTestSuiteTemplates()
        {
            var templates = await _testSuiteTemplateRepository.GetAllTestSuiteTemplatesAsync();
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
            var userName = User.Identity?.Name ?? "System";
            var template = await _testSuiteTemplateRepository.CreateTestSuiteTemplateAsync(request, userName);

            return CreatedAtAction(nameof(GetTestSuiteTemplate), new { id = template.Id }, template);
        }

        // PUT: api/TestSuiteTemplates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestSuiteTemplate(int id, UpdateTestSuiteTemplateRequest request)
        {
            var userName = User.Identity?.Name ?? "System";
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
    }
}