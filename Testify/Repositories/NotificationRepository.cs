using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.Notifications;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId)
        {
            System.Console.WriteLine($"[Repo] GetUserNotificationsAsync - UserId: {userId}");
            
            var allCount = await _context.Notifications.CountAsync(n => n.UserId == userId);
            System.Console.WriteLine($"[Repo] Total matches for User {userId} (ignoring IsDeleted): {allCount}");

            var notifications = await _context.Notifications
                .Include(n => n.Project)
                .Include(n => n.Sender)
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            return notifications.Select(n => MapToResponse(n)).ToList();
        }

        public async Task<NotificationResponse?> GetNotificationByIdAsync(long id)
        {
            var notification = await _context.Notifications
                .Include(n => n.Project)
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);

            return notification != null ? MapToResponse(notification) : null;
        }

        public async Task<bool> AcceptInvitationAsync(long notificationId, string userId, string userName)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null || notification.Type != NotificationType.ProjectInvitation)
                return false;

            if (notification.InvitationStatus != InvitationStatus.Pending)
                return false;

            if (!notification.ProjectId.HasValue)
                return false;

            // Update invitation status
            notification.InvitationStatus = InvitationStatus.Accepted;
            notification.IsRead = true;
            notification.MarkAsUpdated(userName);

            // Add user to project team members
            var existingMember = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == notification.ProjectId && tm.UserId == userId);

            if (existingMember == null)
            {
                var teamMember = new ProjectTeamMember
                {
                    ProjectId = notification.ProjectId.Value,
                    UserId = userId,
                    Role = notification.InvitedRole ?? ProjectRole.Tester, // Use invited role or default to Tester
                    JoinedAt = DateTimeHelper.GetVietnamTime()
                };
                _context.ProjectTeamMembers.Add(teamMember);

                // Update project members count
                var project = await _context.Projects.FindAsync(notification.ProjectId.Value);
                if (project != null)
                {
                    project.MembersCount = await _context.ProjectTeamMembers
                        .CountAsync(tm => tm.ProjectId == notification.ProjectId.Value) + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineInvitationAsync(long notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null || notification.Type != NotificationType.ProjectInvitation)
                return false;

            if (notification.InvitationStatus != InvitationStatus.Pending)
                return false;

            notification.InvitationStatus = InvitationStatus.Declined;
            notification.IsRead = true;
            notification.MarkAsUpdated(userId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReadAsync(long notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.MarkAsUpdated(userId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NotificationResponse> CreateInvitationAsync(
            int projectId,
            string targetUserId,
            string senderUserId,
            string senderName,
            string projectName,
            ProjectRole invitedRole,
            string createdBy)
        {
            System.Console.WriteLine($"[Repo] CreateInvitationAsync - TargetUserId: {targetUserId}, Role: {invitedRole}");
            
            // Get role display name
            var roleDisplay = invitedRole switch
            {
                ProjectRole.Tester => "Tester",
                ProjectRole.Dev => "Developer",
                ProjectRole.PM => "Project Manager",
                _ => "Team Member"
            };
            
            var notification = new Notification
            {
                UserId = targetUserId,
                Title = "Project Invitation",
                Content = $"{senderName} invited you to join project \"{projectName}\" as <strong>{roleDisplay}</strong>",
                Type = NotificationType.ProjectInvitation,
                ProjectId = projectId,
                SenderUserId = senderUserId,
                InvitationStatus = InvitationStatus.Pending,
                InvitedRole = invitedRole,
                IsRead = false,
                Link = $"/projects/{projectId}"
            };

            notification.MarkAsCreated(createdBy);

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToResponse(notification);
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Content = notification.Content,
                Link = notification.Link,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ProjectId = notification.ProjectId,
                ProjectName = notification.Project?.Name,
                SenderName = notification.Sender?.UserName,
                InvitationStatus = notification.InvitationStatus,
                InvitedRole = notification.InvitedRole
            };
        }
    }
}
