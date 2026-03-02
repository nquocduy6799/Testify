using Testify.Entities;

namespace Testify.Interfaces
{
    public interface ITemplateReviewRepository
    {
        Task<TemplateReview?> GetReviewAsync(int templateId, string userId);
        Task<List<int>> GetStarredTemplateIdsAsync(string userId);
        Task<TemplateReview> StarTemplateAsync(int templateId, string userId);
        Task<bool> UnstarTemplateAsync(int templateId, string userId);
        Task<int> GetStarCountAsync(int templateId);
    }
}
