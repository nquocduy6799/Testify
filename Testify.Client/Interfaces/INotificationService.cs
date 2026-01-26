using Testify.Shared.DTOs.Notifications;

namespace Testify.Client.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetNotificationsAsync();
        Task<NotificationResponse?> GetNotificationByIdAsync(long id);
        Task<bool> AcceptInvitationAsync(long notificationId);
        Task<bool> DeclineInvitationAsync(long notificationId);
        Task<bool> MarkAsReadAsync(long notificationId);
    }
}
