using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Services
{
    /// <summary>
    /// On app startup, marks any orphaned Ringing/Active call sessions as Ended.
    /// This handles the case where the server restarted while calls were in progress
    /// (since ActiveUserCalls is in-memory and lost on restart).
    /// </summary>
    public class StaleCallCleanupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StaleCallCleanupService> _logger;

        public StaleCallCleanupService(IServiceProvider serviceProvider, ILogger<StaleCallCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var staleCalls = await context.CallSessions
                .Where(c => c.Status == CallStatus.Ringing || c.Status == CallStatus.Active)
                .ToListAsync(cancellationToken);

            if (staleCalls.Count == 0) return;

            var now = DateTimeHelper.GetVietnamTime();
            foreach (var call in staleCalls)
            {
                call.Status = call.Status == CallStatus.Ringing ? CallStatus.Missed : CallStatus.Ended;
                call.EndedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} stale call session(s) on startup.", staleCalls.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}


