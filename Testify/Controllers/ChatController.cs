using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Testify.Data;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Shared.DTOs.Chat;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatController(IChatRepository chatRepository, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> chatHub)
        {
            _chatRepository = chatRepository;
            _userManager = userManager;
            _chatHub = chatHub;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        [HttpGet("me")]
        public ActionResult<ChatCurrentUserResponse> GetCurrentUser()
        {
            return Ok(new ChatCurrentUserResponse { UserId = GetCurrentUserId() });
        }

        #region Rooms

        // GET: api/chat/rooms
        [HttpGet("rooms")]
        public async Task<ActionResult<List<ChatRoomResponse>>> GetUserChatRooms()
        {
            var userId = GetCurrentUserId();
            var rooms = await _chatRepository.GetUserChatRoomsAsync(userId);
            return Ok(rooms);
        }

        // GET: api/chat/rooms/{id}
        [HttpGet("rooms/{id}")]
        public async Task<ActionResult<ChatRoomResponse>> GetChatRoom(int id)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(id, userId))
                return Forbid();

            var room = await _chatRepository.GetChatRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            return Ok(room);
        }

        // POST: api/chat/rooms
        [HttpPost("rooms")]
        public async Task<ActionResult<ChatRoomResponse>> CreateChatRoom([FromBody] CreateChatRoomRequest request)
        {
            var userId = GetCurrentUserId();
            var room = await _chatRepository.CreateChatRoomAsync(request, userId);
            return CreatedAtAction(nameof(GetChatRoom), new { id = room.Id }, room);
        }

        // POST: api/chat/rooms/direct
        [HttpPost("rooms/direct")]
        public async Task<ActionResult<ChatRoomResponse>> GetOrCreateDirectRoom([FromBody] string otherUserId)
        {
            var userId = GetCurrentUserId();
            var room = await _chatRepository.GetOrCreateDirectRoomAsync(userId, otherUserId);
            
            if (room == null)
                return BadRequest("Could not create direct room");

            return Ok(room);
        }

        // GET: api/chat/users?search=
        [HttpGet("users")]
        public async Task<ActionResult<List<ChatUserItemResponse>>> GetUsersForChat([FromQuery] string? search = null)
        {
            var currentUserId = GetCurrentUserId();
            var users = _userManager.Users
                .Where(u => u.Id != currentUserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                users = users.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)));
            }

            var list = await users
                .OrderBy(u => u.FullName ?? u.UserName ?? u.Email)
                .Take(50)
                .Select(u => new ChatUserItemResponse
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(list);
        }

        #endregion

        #region Messages

        [HttpGet("rooms/{roomId}/messages")]
        public async Task<ActionResult<List<ChatMessageResponse>>> GetRoomMessages(int roomId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                return Forbid();
            var messages = await _chatRepository.GetRoomMessagesAsync(roomId, skip, take);
            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessageResponse>> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(request.RoomId, userId))
                return Forbid();
            var message = await _chatRepository.SendMessageAsync(request, userId);
            // Send to all participants except the sender (sender adds message from HTTP response)
            var participants = await _chatRepository.GetRoomParticipantsAsync(message.RoomId);
            foreach (var p in participants)
            {
                if (p.UserId != userId)
                    await _chatHub.Clients.User(p.UserId).SendAsync("ReceiveMessage", message);
            }
            return Ok(message);
        }

        // DELETE: api/chat/messages/{id}
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = GetCurrentUserId();
            var roomId = await _chatRepository.GetMessageRoomIdAsync(id);
            var success = await _chatRepository.DeleteMessageAsync(id, userId);

            if (!success)
                return NotFound();

            // Broadcast to all participants except sender (sender updates local state)
            if (roomId.HasValue)
            {
                var participants = await _chatRepository.GetRoomParticipantsAsync(roomId.Value);
                foreach (var p in participants)
                {
                    if (p.UserId != userId)
                        await _chatHub.Clients.User(p.UserId).SendAsync("MessageDeleted", id);
                }
            }

            return NoContent();
        }

        [HttpPut("messages/{id}")]
        public async Task<ActionResult<ChatMessageResponse>> UpdateMessage(int id, [FromBody] string newContent)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _chatRepository.UpdateMessageAsync(id, newContent, userId);
                // Broadcast to all participants except sender (sender updates local state from HTTP response)
                var participants = await _chatRepository.GetRoomParticipantsAsync(message.RoomId);
                foreach (var p in participants)
                {
                    if (p.UserId != userId)
                        await _chatHub.Clients.User(p.UserId).SendAsync("MessageUpdated", message);
                }
                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Reactions

        // POST: api/chat/reactions
        [HttpPost("reactions")]
        public async Task<IActionResult> AddReaction([FromBody] AddReactionRequest request)
        {
            var userId = GetCurrentUserId();
            var roomId = await _chatRepository.GetMessageRoomIdAsync(request.MessageId);
            if (!roomId.HasValue)
                return NotFound();
            if (!await _chatRepository.IsUserInRoomAsync(roomId.Value, userId))
                return Forbid();

            var success = await _chatRepository.AddReactionAsync(request, userId);
            if (!success)
                return Conflict("Reaction already exists");

            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.UserName ?? user?.FullName ?? "Unknown";
            // Broadcast to all participants except sender (sender updates local state optimistically)
            var participants = await _chatRepository.GetRoomParticipantsAsync(roomId.Value);
            foreach (var p in participants)
            {
                if (p.UserId != userId)
                    await _chatHub.Clients.User(p.UserId).SendAsync("ReactionAdded", request.MessageId, userId, userName, request.Emoji);
            }

            return Ok();
        }

        // DELETE: api/chat/reactions/{messageId}?emoji=...
        [HttpDelete("reactions/{messageId}")]
        public async Task<IActionResult> RemoveReaction(int messageId, [FromQuery] string emoji)
        {
            var userId = GetCurrentUserId();
            var roomId = await _chatRepository.GetMessageRoomIdAsync(messageId);
            if (!roomId.HasValue)
                return NotFound();
            if (!await _chatRepository.IsUserInRoomAsync(roomId.Value, userId))
                return Forbid();

            var success = await _chatRepository.RemoveReactionAsync(messageId, emoji, userId);
            if (!success)
                return NotFound();

            // Broadcast to all participants except sender
            var participants = await _chatRepository.GetRoomParticipantsAsync(roomId.Value);
            foreach (var p in participants)
            {
                if (p.UserId != userId)
                    await _chatHub.Clients.User(p.UserId).SendAsync("ReactionRemoved", messageId, userId, emoji);
            }

            return NoContent();
        }

        #endregion

        #region Read Receipts

        // POST: api/chat/read
        [HttpPost("read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(request.RoomId, userId))
                return Forbid();
            var marked = await _chatRepository.MarkAsReadAsync(request.RoomId, request.MessageId, userId);
            if (marked)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userName = user?.UserName ?? user?.FullName ?? "Unknown";
                await _chatHub.Clients.Group($"room_{request.RoomId}").SendAsync("MessageRead", request.MessageId, userId, userName);
            }
            return Ok();
        }

        // GET: api/chat/rooms/{roomId}/unread
        [HttpGet("rooms/{roomId}/unread")]
        public async Task<ActionResult<int>> GetUnreadCount(int roomId)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                return Forbid();
            var count = await _chatRepository.GetUnreadCountAsync(roomId, userId);
            return Ok(count);
        }

        #endregion
    }
}
