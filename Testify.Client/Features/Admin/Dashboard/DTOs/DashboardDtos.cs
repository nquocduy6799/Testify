namespace Testify.Client.Features.Admin.Dashboard.DTOs
{
    public class DashboardStatisticsDto
    {
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public int TotalProjects { get; set; }
        public int ActiveTestRuns { get; set; }
        public string SystemUptime { get; set; } = string.Empty;
        public string TotalApiCalls { get; set; } = string.Empty;
        public bool AiServiceAvailable { get; set; }
        public string AiServiceStatus { get; set; } = string.Empty;
    }

    public class RecentActivityDto
    {
        public string Event { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class SystemDiagnosticsDto
    {
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string OverallStatus { get; set; } = "Healthy";
        public PerformanceMetricsDto Performance { get; set; } = new();
        public CapacityAnalysisDto Capacity { get; set; } = new();
        public List<HealthCheckDto> HealthChecks { get; set; } = new();
        public List<RecommendationDto> Recommendations { get; set; } = new();
        public int TotalIssuesFound { get; set; }
        public double DiagnosticDurationMs { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public double AverageApiResponseTimeMs { get; set; }
        public double DatabaseQueryTimeMs { get; set; }
        public int RequestsPerSecond { get; set; }
        public double AiProcessingEfficiency { get; set; }
        public string Status { get; set; } = "Optimal";
    }

    public class CapacityAnalysisDto
    {
        public double NeuralCapacityPercentage { get; set; }
        public double MemoryUsagePercentage { get; set; }
        public double CpuUsagePercentage { get; set; }
        public int ActiveConnections { get; set; }
        public int MaxConnections { get; set; }
        public string LoadStatus { get; set; } = "Normal";
    }

    public class HealthCheckDto
    {
        public string Component { get; set; } = string.Empty;
        public string Status { get; set; } = "Healthy";
        public string Message { get; set; } = string.Empty;
        public double ResponseTimeMs { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class RecommendationDto
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = "Low";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
