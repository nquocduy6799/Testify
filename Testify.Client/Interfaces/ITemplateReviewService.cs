namespace Testify.Client.Interfaces
{
    public interface ITemplateReviewService
    {
        Task<List<int>> GetStarredTemplateIdsAsync();
        Task StarTemplateAsync(int templateId);
        Task UnstarTemplateAsync(int templateId);
        Task<int> GetStarCountAsync(int templateId);
    }
}
