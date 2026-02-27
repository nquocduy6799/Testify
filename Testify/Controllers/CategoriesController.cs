using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Testify.Interfaces;
using Testify.Entities;
using Testify.Shared.DTOs.Categories;
using Testify.Repositories;

namespace Testify.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetAll()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var response = categories.Select(MapToResponse).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(MapToResponse(category));
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest request)
        {
            var category = new TemplateCategory
            {
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId
            };

            var created = await _categoryRepository.CreateCategoryAsync(category);
            var response = MapToResponse(created);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryRepository.DeleteCategoryAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        private static CategoryResponse MapToResponse(TemplateCategory category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name ?? string.Empty,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                SubCategories = category.SubCategories?.Select(MapToResponse).ToList() ?? new()
            };
        }
    }
}
