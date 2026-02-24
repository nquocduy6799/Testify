using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Testify.Data;
using Testify.Interfaces;
using Testify.Shared.DTOs.AI;

namespace Testify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiTestCasesController : ControllerBase
    {
        private readonly IAiTestCaseService _aiService;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>Maximum AI generation uses for free-tier users.</summary>
        private const int FreeTierMaxUses = 3;

        public AiTestCasesController(IAiTestCaseService aiService, UserManager<ApplicationUser> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }

        /// <summary>
        /// Get the current user's AI generation usage.
        /// </summary>
        [HttpGet("usage")]
        public async Task<ActionResult<AiUsageResponse>> GetUsage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            return Ok(new AiUsageResponse
            {
                UsedCount = user.AiGenerationCount,
                MaxCount = FreeTierMaxUses
            });
        }

        [HttpPost("generate")]
        public async Task<ActionResult<AiGenerateTestCasesResponse>> Generate([FromBody] AiGenerateTestCasesRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Check free-tier limit
            if (user.AiGenerationCount >= FreeTierMaxUses)
            {
                return Ok(new AiGenerateTestCasesResponse
                {
                    Success = false,
                    Error = $"You have used all {FreeTierMaxUses} free AI test case generations. Please upgrade your account to continue using this feature.",
                    RemainingUses = 0
                });
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new AiGenerateTestCasesResponse
                {
                    Success = false,
                    Error = "Prompt is required.",
                    RemainingUses = FreeTierMaxUses - user.AiGenerationCount
                });

            if (request.Count < 1 || request.Count > 20)
                request.Count = 5;

            var result = await _aiService.GenerateTestCasesAsync(request);

            if (!result.Success)
            {
                result.RemainingUses = FreeTierMaxUses - user.AiGenerationCount;
                return StatusCode(500, result);
            }

            // Increment usage count on successful generation
            user.AiGenerationCount++;
            await _userManager.UpdateAsync(user);

            result.RemainingUses = FreeTierMaxUses - user.AiGenerationCount;

            return Ok(result);
        }
    }
}
