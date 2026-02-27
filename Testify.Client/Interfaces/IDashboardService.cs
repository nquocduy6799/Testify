using Testify.Shared.Models;

namespace Testify.Shared.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatisticsResult> GetStatisticsAsync();
}
