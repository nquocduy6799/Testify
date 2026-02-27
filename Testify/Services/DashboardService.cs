using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Interfaces;
using Testify.Shared.Models;

namespace Testify.Services;

public class DashboardService : IDashboardService
{
    private static readonly DateTime _startTime = DateTime.UtcNow;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DashboardService(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<DashboardStatisticsResult> GetStatisticsAsync()
    {
        var roles = new[] { "Dev", "Tester", "PM", "Admin" };

        var usersByRole = new Dictionary<string, int>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            usersByRole[role] = usersInRole.Count;
        }

        var totalUsers = await _userManager.Users.CountAsync();
        var totalProjects = await _db.Projects.CountAsync();
        var activeTestRuns = await _db.TestRuns.CountAsync(r => r.Status == TestRunStatus.Untested);

        var uptime = DateTime.UtcNow - _startTime;
        var uptimeStr = uptime.TotalDays >= 1
            ? $"{(int)uptime.TotalDays}d {uptime.Hours}h"
            : $"{uptime.Hours}h {uptime.Minutes}m";

        return new DashboardStatisticsResult
        {
            TotalUsers = totalUsers,
            UsersByRole = usersByRole,
            TotalProjects = totalProjects,
            ActiveTestRuns = activeTestRuns,
            SystemUptime = uptimeStr
        };
    }
}
