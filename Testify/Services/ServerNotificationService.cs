using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Testify.Client.Interfaces;
using Testify.Interfaces;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Services
{
    public class ServerNotificationService : INotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public ServerNotificationService(
            IServiceScopeFactory scopeFactory,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _scopeFactory = scopeFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        private async Task<string?> GetCurrentUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<List<NotificationResponse>> GetNotificationsAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                return new List<NotificationResponse>();
            }

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var notifications = await repo.GetUserNotificationsAsync(userId);
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
