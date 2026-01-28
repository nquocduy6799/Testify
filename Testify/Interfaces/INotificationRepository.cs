using Testify.Shared.DTOs.Notifications;
using Testify.Shared.Enums;

namespace Testify.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId);
        Task<NotificationResponse?> GetNotificationByIdAsync(long id);
        Task<bool> AcceptInvitationAsync(long notificationId, string userId, string userName);
        Task<bool> DeclineInvitationAsync(long notificationId, string userId);
        Task<bool> MarkAsReadAsync(long notificationId, string userId);
        Task<NotificationResponse> CreateInvitationAsync(int projectId, string targetUserId, string senderUserId, string senderName, string projectName, ProjectRole invitedRole, string createdBy);
    }
}
