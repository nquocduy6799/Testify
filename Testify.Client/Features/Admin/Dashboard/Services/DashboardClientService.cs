using System.Net.Http.Json;
using Testify.Client.Features.Admin.Dashboard.DTOs;

namespace Testify.Client.Features.Admin.Dashboard.Services
{
    public interface IDashboardClientService
    {
        Task<DashboardStatisticsDto?> GetStatisticsAsync();
        Task<List<RecentActivityDto>> GetRecentActivityAsync();
    }

    public class DashboardClientService : IDashboardClientService
    {
        private readonly HttpClient _httpClient;

        public DashboardClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DashboardStatisticsDto?> GetStatisticsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DashboardStatisticsDto>("/api/admin/dashboard/statistics");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard statistics: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<RecentActivityDto>>("/api/admin/dashboard/recent-activity");
                return result ?? new List<RecentActivityDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent activity: {ex.Message}");
                return new List<RecentActivityDto>();
            }
        }
    }
}
