using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Meetings;
using Testify.Shared.DTOs.Notifications;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Services
{
    public class MeetingNotificationService : IMeetingNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IUserPresenceService _presence;
        private readonly IAppEmailService _emailService;
        private readonly IMeetingRepository _meetingRepo;
        private readonly ILogger<MeetingNotificationService> _logger;

        public MeetingNotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> notificationHub,
            IUserPresenceService presence,
            IAppEmailService emailService,
            IMeetingRepository meetingRepo,
            ILogger<MeetingNotificationService> logger)
        {
            _context = context;
            _notificationHub = notificationHub;
            _presence = presence;
            _emailService = emailService;
            _meetingRepo = meetingRepo;
            _logger = logger;
        }

        public async Task NotifyMeetingCreatedAsync(MeetingResponse meeting)
        {
            // Get host email to use as sender
            var host = await _context.Users.FindAsync(meeting.HostUserId);
            var hostEmail = host?.Email;
            var hostDisplayName = host?.FullName ?? host?.UserName ?? meeting.HostName;

            foreach (var participant in meeting.Participants)
            {
                // Don't notify the host (they created it)
                if (participant.UserId == meeting.HostUserId) continue;

                // Create in-app notification with link to join
                var notification = new Notification
                {
                    UserId = participant.UserId,
                    Title = "Meeting Invitation",
                    Content = $"<strong>{meeting.HostName}</strong> created a meeting: \"{meeting.Title}\" in project <strong>{meeting.ProjectName}</strong>. Join the waiting room now!",
                    Type = NotificationType.MeetingCreated,
                    ProjectId = meeting.ProjectId,
                    SenderUserId = meeting.HostUserId,
                    IsRead = false,
                    Link = $"/projects/{meeting.ProjectId}?meetingId={meeting.Id}"
                };
                notification.MarkAsCreated(meeting.HostUserId);

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToResponse(notification, meeting.HostName, meeting.ProjectName);

                // Always send SignalR notification (so online users get the Join button)
                await _notificationHub.Clients.Group($"user_{participant.UserId}")
                    .SendAsync("ReceiveNotification", response);

                _logger.LogInformation("Meeting created notification sent via SignalR to {UserId}", participant.UserId);

                // Email notification disabled
                // var user = await _context.Users.FindAsync(participant.UserId);
                // if (user?.Email != null)
                // {
                //     try
                //     {
                //         if (!string.IsNullOrEmpty(hostEmail))
                //         {
                //             await _emailService.SendEmailAsync(
                //                 user.Email,
                //                 $"Meeting Invitation: {meeting.Title}",
                //                 BuildMeetingCreatedEmail(meeting, user.FullName ?? user.UserName ?? "Team Member"),
                //                 hostEmail,
                //                 $"{hostDisplayName} via Testify");
                //         }
                //         else
                //         {
                //             await _emailService.SendEmailAsync(
                //                 user.Email,
                //                 $"Meeting Invitation: {meeting.Title}",
                //                 BuildMeetingCreatedEmail(meeting, user.FullName ?? user.UserName ?? "Team Member"));
                //         }
                //         _logger.LogInformation("Meeting invitation email sent to {Email} from {HostEmail}", user.Email, hostEmail);
                //     }
                //     catch (Exception ex)
                //     {
                //         _logger.LogError(ex, "Failed to send meeting invitation email to {Email}", user.Email);
                //     }
                // }
            }
        }

        public async Task NotifyMeetingStartedAsync(MeetingResponse meeting)
        {
            foreach (var participant in meeting.Participants)
            {
                // Don't notify the host (they started it)
                if (participant.UserId == meeting.HostUserId) continue;

                // Create in-app notification
                var notification = new Notification
                {
                    UserId = participant.UserId,
                    Title = "Meeting Started",
                    Content = $"<strong>{meeting.HostName}</strong> started a meeting: \"{meeting.Title}\" in project <strong>{meeting.ProjectName}</strong>",
                    Type = NotificationType.MeetingStarted,
                    ProjectId = meeting.ProjectId,
                    SenderUserId = meeting.HostUserId,
                    IsRead = false,
                    Link = $"/projects/{meeting.ProjectId}"
                };
                notification.MarkAsCreated(meeting.HostUserId);

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToResponse(notification, meeting.HostName, meeting.ProjectName);

                // Always send SignalR notification (online and offline)
                await _notificationHub.Clients.Group($"user_{participant.UserId}")
                    .SendAsync("ReceiveNotification", response);

                _logger.LogInformation("Meeting notification sent via SignalR to {UserId}", participant.UserId);

                // Email notification disabled
                // if (!_presence.IsOnline(participant.UserId))
                // {
                //     var user = await _context.Users.FindAsync(participant.UserId);
                //     if (user?.Email != null)
                //     {
                //         try
                //         {
                //             await _emailService.SendEmailAsync(
                //                 user.Email,
                //                 $"Meeting Started: {meeting.Title}",
                //                 BuildMeetingStartedEmail(meeting, user.FullName ?? user.UserName ?? "Team Member"));
                //             _logger.LogInformation("Meeting email sent to {Email}", user.Email);
                //         }
                //         catch (Exception ex)
                //         {
                //             _logger.LogError(ex, "Failed to send meeting email to {Email}", user.Email);
                //         }
                //     }
                // }
            }
        }

        public async Task NotifyMeetingSummaryAsync(int meetingId)
        {
            var summary = await _meetingRepo.GetSummaryAsync(meetingId);
            var meeting = await _meetingRepo.GetMeetingByIdAsync(meetingId);
            if (summary == null || meeting == null) return;

            // Generate PDF
            var pdfDoc = new MeetingSummaryPdfDocument(summary);
            var pdfBytes = pdfDoc.GeneratePdf();
            var pdfFileName = $"Meeting_{summary.Title.Replace(" ", "_")}_{summary.StartedAt?.ToString("yyyyMMdd") ?? "draft"}.pdf";

            var attendedUserIds = await _meetingRepo.GetAttendedUserIdsAsync(meetingId);
            var nonAttendedUserIds = await _meetingRepo.GetNonAttendedUserIdsAsync(meetingId);

            // Notify attendees → in-app notification
            foreach (var userId in attendedUserIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Meeting Summary Ready",
                    Content = $"AI summary for \"{meeting.Title}\" is now available.",
                    Type = NotificationType.MeetingSummaryReady,
                    ProjectId = meeting.ProjectId,
                    SenderUserId = meeting.HostUserId,
                    IsRead = false,
                    Link = $"/projects/{meeting.ProjectId}"
                };
                notification.MarkAsCreated("System");

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToResponse(notification, meeting.HostName, meeting.ProjectName);

                await _notificationHub.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", response);
            }

            // Notify non-attendees → email with PDF attachment
            foreach (var userId in nonAttendedUserIds)
            {
                // Also skip the host if they didn't attend (edge case)
                var user = await _context.Users.FindAsync(userId);
                if (user?.Email == null) continue;

                // Create in-app notification
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Meeting Summary (You missed this meeting)",
                    Content = $"You missed the meeting \"{meeting.Title}\". The AI summary has been sent to your email.",
                    Type = NotificationType.MeetingSummaryReady,
                    ProjectId = meeting.ProjectId,
                    SenderUserId = meeting.HostUserId,
                    IsRead = false,
                    Link = $"/projects/{meeting.ProjectId}"
                };
                notification.MarkAsCreated("System");

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToResponse(notification, meeting.HostName, meeting.ProjectName);

                await _notificationHub.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", response);

                // Email with PDF disabled
                // try
                // {
                //     await _emailService.SendEmailWithAttachmentAsync(
                //         user.Email,
                //         $"Meeting Summary: {meeting.Title}",
                //         BuildMeetingSummaryEmail(meeting, summary, user.FullName ?? user.UserName ?? "Team Member"),
                //         pdfBytes,
                //         pdfFileName);
                //     _logger.LogInformation("Meeting summary PDF emailed to {Email} (non-attendee)", user.Email);
                // }
                // catch (Exception ex)
                // {
                //     _logger.LogError(ex, "Failed to send meeting summary email to {Email}", user.Email);
                // }
            }
        }

        private static string BuildMeetingCreatedEmail(MeetingResponse meeting, string recipientName)
        {
            return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #1C1D2A; padding: 32px; border-radius: 16px 16px 0 0; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>Testify</h1>
    </div>
    <div style='background: white; padding: 32px; border: 1px solid #e2e8f0;'>
        <h2 style='color: #1e293b; margin-top: 0;'>Meeting Invitation</h2>
        <p style='color: #64748b;'>Hi <strong>{recipientName}</strong>,</p>
        <p style='color: #64748b;'><strong>{meeting.HostName}</strong> has created a meeting and invited you to join the waiting room.</p>
        <div style='background: #f8fafc; border-radius: 12px; padding: 20px; margin: 20px 0;'>
            <p style='margin: 4px 0; color: #334155;'><strong>Title:</strong> {meeting.Title}</p>
            <p style='margin: 4px 0; color: #334155;'><strong>Project:</strong> {meeting.ProjectName}</p>
            <p style='margin: 4px 0; color: #334155;'><strong>Duration:</strong> {meeting.MaxDurationMinutes} minutes</p>
        </div>
        <p style='color: #64748b;'>Please open Testify and join the waiting room. The meeting will begin once the host starts it.</p>
    </div>
    <div style='background: #f8fafc; padding: 16px; border-radius: 0 0 16px 16px; text-align: center; border: 1px solid #e2e8f0; border-top: none;'>
        <p style='color: #94a3b8; font-size: 12px; margin: 0;'>Sent by {meeting.HostName} via Testify</p>
    </div>
</div>";
        }

        private static string BuildMeetingStartedEmail(MeetingResponse meeting, string recipientName)
        {
            return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #1C1D2A; padding: 32px; border-radius: 16px 16px 0 0; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>Testify</h1>
    </div>
    <div style='background: white; padding: 32px; border: 1px solid #e2e8f0;'>
        <h2 style='color: #1e293b; margin-top: 0;'>Meeting Started</h2>
        <p style='color: #64748b;'>Hi <strong>{recipientName}</strong>,</p>
        <p style='color: #64748b;'><strong>{meeting.HostName}</strong> has started a meeting in project <strong>{meeting.ProjectName}</strong>.</p>
        <div style='background: #f8fafc; border-radius: 12px; padding: 20px; margin: 20px 0;'>
            <p style='margin: 4px 0; color: #334155;'><strong>Title:</strong> {meeting.Title}</p>
            <p style='margin: 4px 0; color: #334155;'><strong>Duration:</strong> {meeting.MaxDurationMinutes} minutes</p>
        </div>
        <p style='color: #64748b;'>Please join the meeting as soon as possible.</p>
    </div>
    <div style='background: #f8fafc; padding: 16px; border-radius: 0 0 16px 16px; text-align: center; border: 1px solid #e2e8f0; border-top: none;'>
        <p style='color: #94a3b8; font-size: 12px; margin: 0;'>Sent by Testify</p>
    </div>
</div>";
        }

        private static string BuildMeetingSummaryEmail(MeetingResponse meeting, MeetingSummaryResponse summary, string recipientName)
        {
            return $@"
<div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #1C1D2A; padding: 32px; border-radius: 16px 16px 0 0; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>Testify</h1>
    </div>
    <div style='background: white; padding: 32px; border: 1px solid #e2e8f0;'>
        <h2 style='color: #1e293b; margin-top: 0;'>Meeting Summary</h2>
        <p style='color: #64748b;'>Hi <strong>{recipientName}</strong>,</p>
        <p style='color: #64748b;'>You missed the meeting <strong>{meeting.Title}</strong> in project <strong>{meeting.ProjectName}</strong>. The AI-generated summary is attached as a PDF.</p>
        <div style='background: #f8fafc; border-radius: 12px; padding: 20px; margin: 20px 0;'>
            <p style='margin: 4px 0; color: #334155;'><strong>Meeting:</strong> {meeting.Title}</p>
            <p style='margin: 4px 0; color: #334155;'><strong>Time:</strong> {summary.StartedAt?.ToString("yyyy-MM-dd HH:mm")} - {summary.EndedAt?.ToString("HH:mm")}</p>
            <p style='margin: 4px 0; color: #334155;'><strong>Attended:</strong> {summary.AttendedCount} / {summary.ParticipantCount} members</p>
        </div>
        <p style='color: #64748b;'>Please review the attached PDF for the full meeting summary.</p>
    </div>
    <div style='background: #f8fafc; padding: 16px; border-radius: 0 0 16px 16px; text-align: center; border: 1px solid #e2e8f0; border-top: none;'>
        <p style='color: #94a3b8; font-size: 12px; margin: 0;'>Sent by Testify AI</p>
    </div>
</div>";
        }

        private static NotificationResponse MapToResponse(Notification n, string? senderName, string? projectName)
        {
            return new NotificationResponse
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Content = n.Content,
                Link = n.Link,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ProjectId = n.ProjectId,
                ProjectName = projectName,
                SenderName = senderName
            };
        }
    }
}
