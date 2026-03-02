using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;

namespace Testify.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;

        public TagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TemplateTag>> GetAllTagsAsync()
        {
            return await _context.TemplateTags
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<TemplateTag?> GetTagByIdAsync(int id)
        {
            return await _context.TemplateTags.FindAsync(id);
        }

        public async Task<TemplateTag?> GetTagByNameAsync(string name)
        {
            return await _context.TemplateTags
                .FirstOrDefaultAsync(t => t.TagName == name);
        }

        public async Task<TemplateTag> CreateTagAsync(TemplateTag tag)
        {
            _context.TemplateTags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.TemplateTags.FindAsync(id);
            if (tag == null) return false;

            _context.TemplateTags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
