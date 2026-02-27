using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.DTOs.Marketplace;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách Templates (Cho trang chủ Marketplace)
        [HttpGet]
        public async Task<ActionResult<List<TemplateDto>>> GetTemplates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var templates = await _context.TestSuiteTemplates
                .Where(t => t.IsPublic)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Title = t.Name,
                    Description = t.Description ?? string.Empty,
                    AuthorName = t.User.UserName ?? "Unknown",
                    AuthorAvatar = "", // TODO: Add avatar URL from User
                    CategoryName = t.Category != null ? t.Category.Name ?? "Uncategorized" : "Uncategorized",
                    Stars = t.TotalStarred,
                    Clones = t.CloneCount,
                    Views = t.ViewCount,
                    PriceType = "Free", // TODO: Add pricing logic when needed
                    PriceAmount = 0,
                    IsOwned = false, // TODO: Check user purchases
                    Tags = t.Tags.Select(tt => tt.Tag.TagName ?? string.Empty).ToList(),
                    UpdatedAt = t.UpdatedAt ?? t.CreatedAt
                })
                .ToListAsync();

            return Ok(templates);
        }

        // 2. Lấy chi tiết một Template (tăng ViewCount)
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateDto>> GetTemplate(int id)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null) return NotFound();

            // Tăng ViewCount
            template.ViewCount++;
            await _context.SaveChangesAsync();

            var dto = new TemplateDto
            {
                Id = template.Id,
                Title = template.Name,
                Description = template.Description ?? string.Empty,
                AuthorName = template.User.UserName ?? "Unknown",
                CategoryName = template.Category?.Name ?? "Uncategorized",
                Stars = template.TotalStarred,
                Clones = template.CloneCount,
                Views = template.ViewCount,
                Tags = template.Tags.Select(tt => tt.Tag.TagName ?? string.Empty).ToList(),
                UpdatedAt = template.UpdatedAt ?? template.CreatedAt
            };

            return Ok(dto);
        }

        // 3. Logic CLONE (Quan trọng nhất)
        [HttpPost("clone")]
        [Authorize]
        public async Task<IActionResult> CloneTemplate([FromBody] CloneTemplateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Kiểm tra user có quyền truy cập Project đích không
            var hasAccess = await _context.ProjectTeamMembers
                .AnyAsync(m => m.ProjectId == request.TargetProjectId && m.UserId == userId);
            
            if (!hasAccess) return Forbid("You don't have access to this project");

            // A. Lấy dữ liệu mẫu từ Template Tables
            var templateSuite = await _context.TestSuiteTemplates
                .Include(t => t.TestCaseTemplates)
                    .ThenInclude(tc => tc.TestStepTemplates)
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

            if (templateSuite == null) return NotFound("Template not found");

            // B. Tạo TestSuite mới cho Project đích
            var newSuite = new TestSuite
            {
                ProjectId = request.TargetProjectId,
                Name = templateSuite.Name,
                Description = templateSuite.Description,
                SourceTemplateId = templateSuite.Id,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TestSuites.Add(newSuite);
            await _context.SaveChangesAsync();

            // C. Copy Test Cases và Test Steps từ Template sang Real Table
            if (templateSuite.TestCaseTemplates != null && templateSuite.TestCaseTemplates.Any())
            {
                foreach (var templateCase in templateSuite.TestCaseTemplates)
                {
                    var newCase = new TestCase
                    {
                        SuiteId = newSuite.Id,
                        Title = templateCase.Title,
                        Priority = templateCase.Priority,
                        Preconditions = templateCase.Preconditions,
                        Postconditions = templateCase.Postconditions,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TestCases.Add(newCase);
                    await _context.SaveChangesAsync();

                    // Copy Test Steps
                    if (templateCase.TestStepTemplates != null)
                    {
                        foreach (var templateStep in templateCase.TestStepTemplates)
                        {
                            var newStep = new TestStep
                            {
                                TestCaseId = newCase.Id,
                                StepNumber = templateStep.StepNumber,
                                Action = templateStep.Action,
                                TestData = templateStep.TestData,
                                ExpectedResult = templateStep.ExpectedResult
                            };
                            _context.TestSteps.Add(newStep);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            // D. Tăng CloneCount của template
            templateSuite.CloneCount++;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cloned successfully", NewSuiteId = newSuite.Id });
        }

        // 4. Star/Unstar Template
        [HttpPost("{id}/star")]
        [Authorize]
        public async Task<IActionResult> ToggleStar(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var template = await _context.TestSuiteTemplates.FindAsync(id);
            if (template == null) return NotFound();

            // TODO: Implement UserTemplateStars table for proper star tracking
            // For now, just increment the count
            template.TotalStarred++;
            await _context.SaveChangesAsync();

            return Ok(new { Stars = template.TotalStarred });
        }

        // ============ ADMIN TEMPLATE CRUD ============

        // Create Template
        [HttpPost("templates")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TemplateDto>> CreateTemplate([FromBody] CreateTemplateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var template = new TestSuiteTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                IsPublic = dto.IsPublic,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.TestSuiteTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Add tags
            if (dto.Tags?.Any() == true)
            {
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _context.TemplateTags.FirstOrDefaultAsync(t => t.TagName == tagName);
                    if (tag == null)
                    {
                        tag = new TemplateTag { TagName = tagName };
                        _context.TemplateTags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    _context.TestSuiteTemplateTags.Add(new TestSuiteTemplateTag
                    {
                        TemplateId = template.Id,
                        TagId = tag.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new TemplateDto
            {
                Id = template.Id,
                Title = template.Name,
                Description = template.Description ?? string.Empty,
                CategoryName = (await _context.TemplateCategories.FindAsync(template.CategoryId))?.Name ?? "Uncategorized",
                Tags = dto.Tags ?? new(),
                UpdatedAt = template.CreatedAt
            });
        }

        // Update Template
        [HttpPut("templates/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TemplateDto>> UpdateTemplate(int id, [FromBody] CreateTemplateDto dto)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.CategoryId = dto.CategoryId;
            template.IsPublic = dto.IsPublic;
            template.UpdatedAt = DateTime.UtcNow;

            // Update tags - remove old, add new
            _context.TestSuiteTemplateTags.RemoveRange(template.Tags);
            if (dto.Tags?.Any() == true)
            {
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _context.TemplateTags.FirstOrDefaultAsync(t => t.TagName == tagName);
                    if (tag == null)
                    {
                        tag = new TemplateTag { TagName = tagName };
                        _context.TemplateTags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    _context.TestSuiteTemplateTags.Add(new TestSuiteTemplateTag
                    {
                        TemplateId = template.Id,
                        TagId = tag.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new TemplateDto
            {
                Id = template.Id,
                Title = template.Name,
                Description = template.Description ?? string.Empty,
                CategoryName = (await _context.TemplateCategories.FindAsync(template.CategoryId))?.Name ?? "Uncategorized",
                Tags = dto.Tags ?? new(),
                UpdatedAt = template.UpdatedAt ?? template.CreatedAt
            });
        }

        // Delete Template
        [HttpDelete("templates/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.TestSuiteTemplates
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return NotFound();

            _context.TestSuiteTemplateTags.RemoveRange(template.Tags);
            _context.TestSuiteTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Get ALL templates (Admin - includes non-public)
        [HttpGet("templates/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TemplateDto>>> GetAllTemplates()
        {
            var templates = await _context.TestSuiteTemplates
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Title = t.Name,
                    Description = t.Description ?? string.Empty,
                    AuthorName = t.User.UserName ?? "Unknown",
                    CategoryName = t.Category != null ? t.Category.Name ?? "Uncategorized" : "Uncategorized",
                    Stars = t.TotalStarred,
                    Clones = t.CloneCount,
                    Views = t.ViewCount,
                    PriceType = t.IsPublic ? "Public" : "Private",
                    Tags = t.Tags.Select(tt => tt.Tag.TagName ?? string.Empty).ToList(),
                    UpdatedAt = t.UpdatedAt ?? t.CreatedAt
                })
                .ToListAsync();

            return Ok(templates);
        }

        // ============ CATEGORIES ENDPOINTS ============

        // 5. Lấy danh sách Categories
        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories()
        {
            var categories = await _context.TemplateCategories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Subcategories = c.SubCategories.Select(s => s.Name ?? string.Empty).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // 6. Tạo Category mới
        [HttpPost("categories")]
        [Authorize]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryDto dto)
        {
            var category = new TemplateCategory
            {
                Name = dto.Name
            };

            _context.TemplateCategories.Add(category);
            await _context.SaveChangesAsync();

            dto.Id = category.Id;
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, dto);
        }

        // 7. Cập nhật Category
        [HttpPut("categories/{id}")]
        [Authorize]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            var category = await _context.TemplateCategories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // 8. Xóa Category
        [HttpDelete("categories/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.TemplateCategories.FindAsync(id);
            if (category == null) return NotFound();

            _context.TemplateCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
