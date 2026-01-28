using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Testify.Data;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Invitations;
using Testify.Shared.Enums;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public InvitationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // POST: api/invitations/send
        [HttpPost("send")]
        public async Task<ActionResult<InvitationResponse>> SendInvitation(SendInvitationRequest request)
        {
            // Get current user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserName = User.Identity?.Name ?? "System";

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new InvitationResponse
                {
                    Success = false,
                    Message = "You must be logged in to send invitations"
                });
            }

            // Get current user's full name
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var senderName = currentUser?.FullName ?? currentUser?.UserName ?? "A team member";

            // Validate project exists
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted);

            if (project == null)
            {
                return NotFound(new InvitationResponse
                {
                    Success = false,
                    Message = "Project not found"
                });
            }

            // Check if current user is PM of the project (has permission to invite)
            var currentUserRole = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == request.ProjectId && tm.UserId == currentUserId);

            if (currentUserRole == null || currentUserRole.Role != ProjectRole.PM)
            {
                return StatusCode(403, new InvitationResponse
                {
                    Success = false,
                    Message = "Only Project Managers can invite members"
                });
            }

            // Find user by email
            var targetUser = await _userManager.FindByEmailAsync(request.Email);

            if (targetUser == null)
            {
                return NotFound(new InvitationResponse
                {
                    Success = false,
                    Message = $"No user found with email: {request.Email}"
                });
            }

            // Check if user is already a member of the project
            var existingMember = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == request.ProjectId && tm.UserId == targetUser.Id);

            if (existingMember != null)
            {
                return BadRequest(new InvitationResponse
                {
                    Success = false,
                    Message = "This user is already a member of the project"
                });
            }

            // Check if there's already a pending invitation for this user to this project
            var existingInvitation = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.UserId == targetUser.Id &&
                    n.ProjectId == request.ProjectId &&
                    n.Type == NotificationType.ProjectInvitation &&
                    n.InvitationStatus == InvitationStatus.Pending &&
                    !n.IsDeleted);

            if (existingInvitation != null)
            {
                return BadRequest(new InvitationResponse
                {
                    Success = false,
                    Message = "An invitation is already pending for this user"
                });
            }

            // Create the invitation notification
            // FIXED: Ensuring UserId is assigned to the RECIPIENT (targetUser.Id), not the sender.
            Console.WriteLine($"[Invitations] Creating invitation for Recipient: {targetUser.Email} (ID: {targetUser.Id}) from Sender: {currentUserName} (ID: {currentUserId})");

            var notification = await _notificationRepository.CreateInvitationAsync(
                projectId: request.ProjectId,
                targetUserId: targetUser.Id, // <--- VERIFIED: This is the Recipient's ID
                senderUserId: currentUserId,
                senderName: senderName,
                projectName: project.Name,
                invitedRole: request.Role,
                createdBy: currentUserName);

            // Send real-time notification via SignalR
            await _hubContext.Clients.Group($"user_{targetUser.Id}")
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[SignalR] Sent notification to user_{targetUser.Id}");

            return Ok(new InvitationResponse
            {
                Success = true,
                Message = $"Invitation sent successfully to {request.Email}",
                NotificationId = notification.Id
            });
        }

        // GET: api/invitations/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetProjectInvitations(int projectId)
        {
            var invitations = await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Sender)
                .Where(n =>
                    n.ProjectId == projectId &&
                    n.Type == NotificationType.ProjectInvitation &&
                    !n.IsDeleted)
                .Select(n => new
                {
                    n.Id,
                    InviteeEmail = n.User.Email,
                    InviteeName = n.User.FullName ?? n.User.UserName,
                    SenderName = n.Sender != null ? (n.Sender.FullName ?? n.Sender.UserName) : "Unknown",
                    Status = n.InvitationStatus,
                    SentAt = n.CreatedAt
                })
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return Ok(invitations);
        }

        // GET: api/invitations/project/{projectId}/pending
        [HttpGet("project/{projectId}/pending")]
        public async Task<ActionResult<IEnumerable<PendingInvitationResponse>>> GetPendingInvitations(int projectId)
        {
            var invitations = await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Sender)
                .Where(n =>
                    n.ProjectId == projectId &&
                    n.Type == NotificationType.ProjectInvitation &&
                    n.InvitationStatus == InvitationStatus.Pending &&
                    !n.IsDeleted)
                .Select(n => new PendingInvitationResponse
                {
                    Id = n.Id,
                    Email = n.User.Email ?? "",
                    InviteeName = n.User.FullName ?? n.User.UserName,
                    SenderName = n.Sender != null ? (n.Sender.FullName ?? n.Sender.UserName) : "Unknown",
                    Role = ProjectRole.Tester, // Default role, could be stored in notification metadata
                    Status = n.InvitationStatus ?? InvitationStatus.Pending,
                    SentAt = n.CreatedAt
                })
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return Ok(invitations);
        }

        // DELETE: api/invitations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RevokeInvitation(long id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.Type == NotificationType.ProjectInvitation && !n.IsDeleted);

            if (notification == null)
            {
                return NotFound(new { message = "Invitation not found" });
            }

            // Check if current user is PM of the project
            var currentUserRole = await _context.ProjectTeamMembers
                .FirstOrDefaultAsync(tm => tm.ProjectId == notification.ProjectId && tm.UserId == currentUserId);

            if (currentUserRole == null || currentUserRole.Role != ProjectRole.PM)
            {
                return StatusCode(403, new { message = "Only Project Managers can revoke invitations" });
            }

            notification.MarkAsDeleted(currentUserId);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invitation revoked successfully" });
        }
    }
}
