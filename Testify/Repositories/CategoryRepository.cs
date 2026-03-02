using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;

namespace Testify.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TemplateCategory>> GetAllCategoriesAsync()
        {
            return await _context.TemplateCategories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<TemplateCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.TemplateCategories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<TemplateCategory> CreateCategoryAsync(TemplateCategory category)
        {
            _context.TemplateCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<TemplateCategory> UpdateCategoryAsync(TemplateCategory category)
        {
            _context.TemplateCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.TemplateCategories.FindAsync(id);
            if (category == null) return false;

            _context.TemplateCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
