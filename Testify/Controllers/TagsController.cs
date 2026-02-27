using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Entities;
using Testify.Shared.DTOs.Tags;

namespace Testify.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;

        public TagsController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<TagResponse>>> GetAll()
        {
            var tags = await _tagRepository.GetAllTagsAsync();
            var response = tags.Select(t => new TagResponse
            {
                Id = t.Id,
                TagName = t.TagName ?? string.Empty
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponse>> GetById(int id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            var response = new TagResponse
            {
                Id = tag.Id,
                TagName = tag.TagName ?? string.Empty
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<TagResponse>> Create([FromBody] CreateTagRequest request)
        {
            // Check if tag already exists
            var existing = await _tagRepository.GetTagByNameAsync(request.TagName);
            if (existing != null)
            {
                return Conflict("Tag already exists");
            }

            var tag = new TemplateTag
            {
                TagName = request.TagName
            };

            var created = await _tagRepository.CreateTagAsync(tag);
            var response = new TagResponse
            {
                Id = created.Id,
                TagName = created.TagName ?? string.Empty
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tagRepository.DeleteTagAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
