using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Testify.Client.Interfaces;
using Testify.Interfaces;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Repositories
{
    public class ServerNotificationRepository : INotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public ServerNotificationRepository(
            IServiceScopeFactory scopeFactory,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _scopeFactory = scopeFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        private async Task<string?> GetCurrentUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"[ServerNotificationService] GetCurrentUserIdAsync: {userId ?? "NULL"}");
            return userId;
        }

        public async Task<List<NotificationResponse>> GetNotificationsAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            Console.WriteLine($"[ServerNotificationService] Getting notifications for userId: {userId ?? "NULL"}");
            
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[ServerNotificationService] UserId is null, returning empty list");
                return new List<NotificationResponse>();
            }

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var notifications = await repo.GetUserNotificationsAsync(userId);
            Console.WriteLine($"[ServerNotificationService] Found {notifications.Count()} notifications");
            return notifications.ToList();
        }

        public async Task<NotificationResponse?> GetNotificationByIdAsync(long id)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repo.GetNotificationByIdAsync(id);
        }

        public async Task<bool> AcceptInvitationAsync(long notificationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return false;

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userName = authState.User.Identity?.Name ?? "User";

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repo.AcceptInvitationAsync(notificationId, userId, userName);
        }

        public async Task<bool> DeclineInvitationAsync(long notificationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return false;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repo.DeclineInvitationAsync(notificationId, userId);
        }

        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return false;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            return await repo.MarkAsReadAsync(notificationId, userId);
        }
    }
}
