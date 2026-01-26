using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            System.Console.WriteLine($"[Controller] GetNotifications - UserId Claim: {userId ?? "NULL"}");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponse>> GetNotification(long id)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }

        // POST: api/Notifications/{id}/accept
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptInvitation(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "System";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _notificationRepository.AcceptInvitationAsync(id, userId, userName);

            if (!success)
            {
                return BadRequest("Failed to accept invitation. It may have already been processed or doesn't exist.");
            }

            return Ok(new { message = "Invitation accepted successfully" });
        }

        // POST: api/Notifications/{id}/decline
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineInvitation(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _notificationRepository.DeclineInvitationAsync(id, userId);

            if (!success)
            {
                return BadRequest("Failed to decline invitation. It may have already been processed or doesn't exist.");
            }

            return Ok(new { message = "Invitation declined" });
        }

        // POST: api/Notifications/{id}/read
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _notificationRepository.MarkAsReadAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return Ok(new { message = "Notification marked as read" });
        }
    }
}
