using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Meetings;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepo;
        private readonly IMeetingNotificationService _notificationService;
        private readonly IHubContext<MeetingHub> _meetingHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(
            IMeetingRepository meetingRepo,
            IMeetingNotificationService notificationService,
            IHubContext<MeetingHub> meetingHub,
            IHubContext<NotificationHub> notificationHub,
            IServiceScopeFactory scopeFactory,
            ILogger<MeetingsController> logger)
        {
            _meetingRepo = meetingRepo;
            _notificationService = notificationService;
            _meetingHub = meetingHub;
            _notificationHub = notificationHub;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        [HttpPost]
        public async Task<ActionResult<MeetingResponse>> CreateMeeting([FromBody] CreateMeetingRequest request)
        {
            var userId = GetCurrentUserId();

            if (!await _meetingRepo.IsUserInProjectAsync(request.ProjectId, userId))
                return Forbid();

            var meeting = await _meetingRepo.CreateMeetingAsync(request, userId);

            // Notify all project members about the new meeting (email from host, SignalR for online)
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IMeetingNotificationService>();
                try
                {
                    await notificationService.NotifyMeetingCreatedAsync(meeting);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send meeting created notifications for meeting {MeetingId}", meeting.Id);
                }
            });

            return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, meeting);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingResponse>> GetMeeting(int id)
        {
            var meeting = await _meetingRepo.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound();

            var userId = GetCurrentUserId();
            if (!await _meetingRepo.IsUserInProjectAsync(meeting.ProjectId, userId))
                return Forbid();

            return Ok(meeting);
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<MeetingResponse>>> GetProjectMeetings(int projectId)
        {
            var userId = GetCurrentUserId();
            if (!await _meetingRepo.IsUserInProjectAsync(projectId, userId))
                return Forbid();

            var meetings = await _meetingRepo.GetProjectMeetingsAsync(projectId);
            return Ok(meetings);
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult<MeetingResponse>> StartMeeting(int id)
        {
            var userId = GetCurrentUserId();

            var meeting = await _meetingRepo.StartMeetingAsync(id, userId);
            if (meeting == null)
                return BadRequest("Cannot start meeting. Either not found, not the host, or already started.");

            // Notify participants in the waiting room via MeetingHub
            await _meetingHub.Clients.Group($"meeting_{id}")
                .SendAsync("MeetingStarted", meeting);

            // Notify all project members via NotificationHub (real-time UI update)
            await _notificationHub.Clients.Group($"project_{meeting.ProjectId}")
                .SendAsync("MeetingStarted", meeting);

            // Send notifications (online → SignalR, offline → email)
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IMeetingNotificationService>();
                try
                {
                    await notificationService.NotifyMeetingStartedAsync(meeting);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send meeting start notifications for meeting {MeetingId}", id);
                }
            });

            _logger.LogInformation("Meeting {MeetingId} started by {UserId}", id, userId);
            return Ok(meeting);
        }

        [HttpPost("{id}/end")]
        public async Task<ActionResult<MeetingResponse>> EndMeeting(int id)
        {
            var userId = GetCurrentUserId();

            var meeting = await _meetingRepo.EndMeetingAsync(id, userId);
            if (meeting == null)
                return BadRequest("Cannot end meeting. Either not found, not the host, or not in progress.");

            // Notify participants
            await _meetingHub.Clients.Group($"meeting_{id}")
                .SendAsync("MeetingEnded", id);

            // Notify participants via NotificationHub (real-time UI update) in background
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IMeetingNotificationService>();
                try
                {
                    await notificationService.NotifyMeetingSummaryAsync(id);
                    _logger.LogInformation("End notifications sent for meeting {MeetingId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send end notifications for meeting {MeetingId}", id);
                }
            });

            _logger.LogInformation("Meeting {MeetingId} ended by {UserId}", id, userId);
            return Ok(meeting);
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinMeeting(int id)
        {
            var userId = GetCurrentUserId();

            var success = await _meetingRepo.JoinMeetingAsync(id, userId);
            if (!success)
                return BadRequest("Cannot join meeting.");

            return Ok(new { message = "Joined meeting successfully" });
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveMeeting(int id)
        {
            var userId = GetCurrentUserId();

            var success = await _meetingRepo.LeaveMeetingAsync(id, userId);
            if (!success)
                return BadRequest("Cannot leave meeting.");

            return Ok(new { message = "Left meeting successfully" });
        }

        [HttpGet("{id}/transcripts")]
        public async Task<ActionResult<List<MeetingTranscriptEntry>>> GetTranscripts(int id)
        {
            var meeting = await _meetingRepo.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound();

            var userId = GetCurrentUserId();
            if (!await _meetingRepo.IsUserInProjectAsync(meeting.ProjectId, userId))
                return Forbid();

            var transcripts = await _meetingRepo.GetTranscriptsAsync(id);
            return Ok(transcripts);
        }

        [HttpPost("{id}/transcripts")]
        public async Task<IActionResult> AddTranscript(int id, [FromBody] Shared.DTOs.Meetings.AddTranscriptRequest request)
        {
            var meeting = await _meetingRepo.GetMeetingByIdAsync(id);
            if (meeting == null) return NotFound();

            var userId = GetCurrentUserId();
            if (!await _meetingRepo.IsUserInProjectAsync(meeting.ProjectId, userId))
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Content cannot be empty.");

            var entry = await _meetingRepo.AddTranscriptAsync(id, userId, request.Content);
            return Ok(entry);
        }
    }
}
