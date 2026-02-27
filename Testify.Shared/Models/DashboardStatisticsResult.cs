namespace Testify.Shared.Models;

public class DashboardStatisticsResult
{
    public int TotalUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public int TotalProjects { get; set; }
    public int ActiveTestRuns { get; set; }
    public string SystemUptime { get; set; } = string.Empty;
    public string TotalApiCalls { get; set; } = "N/A";
}
