using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Shared.DTOs.TestCases;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCaseTemplatesController : ControllerBase
    {
        private readonly ITestCaseTemplateRepository _repository;
        private readonly ICurrentUserRepository _currentUserRepository;

        public TestCaseTemplatesController(ITestCaseTemplateRepository repository, ICurrentUserRepository currentUserRepository)
        {
            _repository = repository;
            _currentUserRepository = currentUserRepository;
        }

        // GET: api/TestCaseTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCaseTemplateResponse>> GetTestCaseTemplate(int id)
        {
            var template = await _repository.GetTestCaseTemplateByIdAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return Ok(template);
        }

        [HttpPost("~/api/testsuitetemplates/{suiteId}/testcases")]
        public async Task<ActionResult<TestCaseTemplateResponse>> CreateTestCaseTemplate(int suiteId, CreateTestCaseTemplateRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var template = await _repository.CreateTestCaseTemplateAsync(suiteId, request, userId);

            return CreatedAtAction(nameof(GetTestCaseTemplate), new { id = template.Id }, template);
        }

        // PUT: api/TestCaseTemplates/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestCaseTemplate(int id, UpdateTestCaseTemplateRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var result = await _repository.UpdateTestCaseTemplateAsync(id, request, userId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/TestCaseTemplates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestCaseTemplate(int id)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var result = await _repository.DeleteTestCaseTemplateAsync(id, userId);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
