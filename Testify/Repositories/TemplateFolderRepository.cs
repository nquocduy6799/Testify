using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.TemplateFolders;

namespace Testify.Repositories
{
    public class TemplateFolderRepository : ITemplateFolderRepository
    {
        private readonly ApplicationDbContext _context;

        public TemplateFolderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TemplateFolderResponse?> GetTemplateFolderByIdAsync(int id)
        {
            return await _context.TemplateFolders
                .Where(tf => tf.Id == id)
                .Select(tf => new TemplateFolderResponse
                {
                    Id = tf.Id,
                    Name = tf.Name,
                    Description = tf.Description,
                    ParentId = tf.ParentId,
                    SubFolders = tf.SubFolders.Select(sf => new TemplateFolderResponse
                    {
                        Id = sf.Id,
                        Name = sf.Name,
                        Description = sf.Description,
                        ParentId = sf.ParentId,
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TemplateFolderResponse>> GetAllTemplateFoldersAsync()
        {
            var folders = await _context.TemplateFolders
                .Include(tf => tf.SubFolders)
                .Where(tf => tf.ParentId == null) // Only get root folders
                .ToListAsync();

            return folders.Select(tf => MapToResponse(tf)).ToList();
        }

        public async Task<TemplateFolderResponse> CreateTemplateFolderAsync(CreateTemplateFolderRequest request, string userName, string userId)
        {
            var folder = new TemplateFolder
            {
                Name = request.Name,
                Description = request.Description,
                ParentId = request.ParentId,
                UserId = userId,
            };

            _context.TemplateFolders.Add(folder);
            await _context.SaveChangesAsync();

            return MapToResponse(folder);
        }

        public async Task<bool> UpdateTemplateFolderAsync(int id, UpdateTemplateFolderRequest request, string userName)
        {
            var folder = await _context.TemplateFolders.FindAsync(id);

            if (folder == null)
                return false;

            folder.Name = request.Name;
            folder.Description = request.Description;
            folder.ParentId = request.ParentId;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TemplateFolderExistsAsync(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteTemplateFolderAsync(int id, string userName)
        {
            var folder = await _context.TemplateFolders
                .Include(tf => tf.SubFolders)
                .FirstOrDefaultAsync(tf => tf.Id == id);

            if (folder == null)
                return false;

            // Check if folder has subfolders
            if (folder.SubFolders.Any())
                return false;

            _context.TemplateFolders.Remove(folder);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> TemplateFolderExistsAsync(int id)
        {
            return await _context.TemplateFolders.AnyAsync(tf => tf.Id == id);
        }

        private static TemplateFolderResponse MapToResponse(TemplateFolder folder)
        {
            return new TemplateFolderResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                ParentId = folder.ParentId,
                SubFolders = folder.SubFolders?.Select(sf => new TemplateFolderResponse
                {
                    Id = sf.Id,
                    Name = sf.Name,
                    Description = sf.Description,
                    ParentId = sf.ParentId,
                }).ToList() ?? new List<TemplateFolderResponse>()
            };
        }
    }
}