using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class TemplateReviewRepository : ITemplateReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public TemplateReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TemplateReview?> GetReviewAsync(int templateId, string userId)
        {
            return await _context.TemplateReviews
                .FirstOrDefaultAsync(r => r.TemplateId == templateId && r.UserId == userId);
        }

        public async Task<List<int>> GetStarredTemplateIdsAsync(string userId)
        {
            return await _context.TemplateReviews
                .Where(r => r.UserId == userId && r.IsStarred)
                .Select(r => r.TemplateId)
                .ToListAsync();
        }

        public async Task<TemplateReview> StarTemplateAsync(int templateId, string userId)
        {
            var existingReview = await GetReviewAsync(templateId, userId);

            if (existingReview != null)
            {
                existingReview.IsStarred = true;
                existingReview.CreatedAt = DateTimeHelper.GetVietnamTime();
            }
            else
            {
                existingReview = new TemplateReview
                {
                    TemplateId = templateId,
                    UserId = userId,
                    IsStarred = true,
                    CreatedAt = DateTimeHelper.GetVietnamTime()
                };
                _context.TemplateReviews.Add(existingReview);
            }

            await _context.SaveChangesAsync();

            // Update TotalStarred count in a separate operation
            var template = await _context.TestSuiteTemplates.FindAsync(templateId);
            if (template != null)
            {
                template.TotalStarred = await _context.TemplateReviews
                    .CountAsync(r => r.TemplateId == templateId && r.IsStarred);
                await _context.SaveChangesAsync();
            }

            return existingReview;
        }

        public async Task<bool> UnstarTemplateAsync(int templateId, string userId)
        {
            var review = await GetReviewAsync(templateId, userId);
            if (review == null) return false;

            review.IsStarred = false;
            await _context.SaveChangesAsync();

            // Update TotalStarred count in a separate operation
            var template = await _context.TestSuiteTemplates.FindAsync(templateId);
            if (template != null)
            {
                template.TotalStarred = await _context.TemplateReviews
                    .CountAsync(r => r.TemplateId == templateId && r.IsStarred);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> GetStarCountAsync(int templateId)
        {
            return await _context.TemplateReviews
                .CountAsync(r => r.TemplateId == templateId && r.IsStarred);
        }
    }
}
