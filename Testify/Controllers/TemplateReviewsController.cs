using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TemplateReviewsController : ControllerBase
    {
        private readonly ITemplateReviewRepository _templateReviewRepository;
        private readonly ICurrentUserRepository _currentUserRepository;

        public TemplateReviewsController(
            ITemplateReviewRepository templateReviewRepository,
            ICurrentUserRepository currentUserRepository)
        {
            _templateReviewRepository = templateReviewRepository;
            _currentUserRepository = currentUserRepository;
        }

        // GET: api/TemplateReviews/starred
        [HttpGet("starred")]
        public async Task<ActionResult<List<int>>> GetStarredTemplateIds()
        {
            var userId = _currentUserRepository.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var starredIds = await _templateReviewRepository.GetStarredTemplateIdsAsync(userId);
            return Ok(starredIds);
        }

        // POST: api/TemplateReviews/{templateId}/star
        [HttpPost("{templateId}/star")]
        public async Task<IActionResult> StarTemplate(int templateId)
        {
            var userId = _currentUserRepository.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _templateReviewRepository.StarTemplateAsync(templateId, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/TemplateReviews/{templateId}/star
        [HttpDelete("{templateId}/star")]
        public async Task<IActionResult> UnstarTemplate(int templateId)
        {
            var userId = _currentUserRepository.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _templateReviewRepository.UnstarTemplateAsync(templateId, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/TemplateReviews/{templateId}/stars/count
        [HttpGet("{templateId}/stars/count")]
        public async Task<ActionResult<int>> GetStarCount(int templateId)
        {
            var count = await _templateReviewRepository.GetStarCountAsync(templateId);
            return Ok(count);
        }
    }
}
