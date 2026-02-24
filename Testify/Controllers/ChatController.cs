using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Testify.Data;
using Testify.Entities;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Settings;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

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
        private readonly IFileStorageService _fileStorage;
        private readonly FileUploadSettings _uploadSettings;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatRepository chatRepository,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> chatHub,
            IFileStorageService fileStorage,
            IOptions<FileUploadSettings> uploadSettings,
            ILogger<ChatController> logger)
        {
            _chatRepository = chatRepository;
            _userManager = userManager;
            _chatHub = chatHub;
            _fileStorage = fileStorage;
            _uploadSettings = uploadSettings.Value;
            _logger = logger;
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

        // PUT: api/chat/rooms/{roomId}
        [HttpPut("rooms/{roomId}")]
        public async Task<ActionResult<ChatRoomResponse>> UpdateRoom(int roomId, [FromBody] UpdateRoomRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                // Only admin/owner can rename
                var role = await _chatRepository.GetUserRoleInRoomAsync(roomId, userId);
                if (role != ChatParticipantRole.Admin && role != ChatParticipantRole.Owner)
                    return Forbid("Only admins can update room settings");

                var room = await _chatRepository.UpdateRoomAsync(roomId, request, userId);
                if (room == null)
                    return BadRequest("Cannot update this room");

                // Broadcast to room via SignalR
                await _chatHub.Clients.Group($"room_{roomId}").SendAsync("RoomUpdated", room);

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/chat/rooms/{roomId}/leave
        [HttpPost("rooms/{roomId}/leave")]
        public async Task<ActionResult> LeaveRoom(int roomId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                var user = await _userManager.FindByIdAsync(userId);
                var result = await _chatRepository.LeaveRoomAsync(roomId, userId);
                if (!result)
                    return BadRequest("Cannot leave this room");

                // Broadcast to room
                await _chatHub.Clients.Group($"room_{roomId}").SendAsync("ParticipantRemoved", roomId, userId, user?.FullName ?? user?.UserName ?? "");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/chat/rooms/{roomId}/members
        [HttpPost("rooms/{roomId}/members")]
        public async Task<ActionResult> AddMembers(int roomId, [FromBody] AddMembersRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                // Only admin/owner can add members
                var role = await _chatRepository.GetUserRoleInRoomAsync(roomId, userId);
                if (role != ChatParticipantRole.Admin && role != ChatParticipantRole.Owner)
                    return Forbid("Only admins can add members");

                var addedUsers = new List<ChatParticipantResponse>();
                foreach (var memberId in request.UserIds)
                {
                    var added = await _chatRepository.AddParticipantAsync(roomId, memberId, userId);
                    if (added)
                    {
                        var user = await _userManager.FindByIdAsync(memberId);
                        if (user != null)
                        {
                            addedUsers.Add(new ChatParticipantResponse
                            {
                                UserId = memberId,
                                UserName = user.UserName ?? "",
                                FullName = user.FullName,
                                AvatarUrl = user.AvatarUrl,
                                Role = ChatParticipantRole.Member,
                                JoinedAt = DateTimeHelper.GetVietnamTime()
                            });
                        }
                    }
                }

                if (addedUsers.Any())
                {
                    // Broadcast to room
                    await _chatHub.Clients.Group($"room_{roomId}").SendAsync("ParticipantsAdded", roomId, addedUsers);
                }

                return Ok(addedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding members to room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/chat/rooms/{roomId}/members/{memberId}
        [HttpDelete("rooms/{roomId}/members/{memberId}")]
        public async Task<ActionResult> RemoveMember(int roomId, string memberId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                // Only admin/owner can remove others
                var role = await _chatRepository.GetUserRoleInRoomAsync(roomId, userId);
                if (role != ChatParticipantRole.Admin && role != ChatParticipantRole.Owner)
                    return Forbid("Only admins can remove members");

                // Can't remove yourself via this endpoint (use leave)
                if (memberId == userId)
                    return BadRequest("Use the leave endpoint instead");

                var user = await _userManager.FindByIdAsync(memberId);
                var result = await _chatRepository.RemoveParticipantAsync(roomId, memberId, userId);
                if (!result)
                    return BadRequest("Cannot remove this member");

                await _chatHub.Clients.Group($"room_{roomId}").SendAsync("ParticipantRemoved", roomId, memberId, user?.FullName ?? user?.UserName ?? "");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member from room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/chat/rooms/{roomId}/mute
        [HttpPost("rooms/{roomId}/mute")]
        public async Task<ActionResult> ToggleMute(int roomId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                var result = await _chatRepository.ToggleMuteAsync(roomId, userId);
                if (!result)
                    return BadRequest("Cannot toggle mute");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling mute for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Messages

        [HttpGet("rooms/{roomId}/messages")]
        public async Task<ActionResult<List<ChatMessageResponse>>> GetRoomMessages(int roomId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var userId = GetCurrentUserId();
            if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                return Forbid();
            var clampedTake = Math.Clamp(take, 1, 100);
            var messages = await _chatRepository.GetRoomMessagesAsync(roomId, skip, clampedTake);
            return Ok(messages);
        }

        [HttpGet("rooms/{roomId}/messages/search")]
        public async Task<ActionResult<SearchMessagesResponse>> SearchMessages(int roomId, [FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest("Search query cannot be empty.");

                var currentUserId = GetCurrentUserId();

                if (!await _chatRepository.IsUserInRoomAsync(roomId, currentUserId))
                    return Forbid();

                var clampedTake = Math.Clamp(take, 1, 50);
                var (messages, totalCount) = await _chatRepository.SearchMessagesAsync(roomId, query.Trim(), skip, clampedTake);

                return Ok(new SearchMessagesResponse { Messages = messages, TotalCount = totalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching messages in room {RoomId} with query '{Query}'", roomId, query);
                return StatusCode(500, "An error occurred while searching messages.");
            }
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
            var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
            if (otherUserIds.Any())
                await _chatHub.Clients.Users(otherUserIds).SendAsync("ReceiveMessage", message);

            return Ok(message);
        }

        /// <summary>
        /// Send a message with file attachments (multipart/form-data).
        /// </summary>
        [HttpPost("messages/with-attachments")]
        [RequestSizeLimit(250 * 1024 * 1024)] // 250 MB total limit
        public async Task<ActionResult<ChatMessageResponse>> SendMessageWithAttachments(
            [FromForm] int roomId,
            [FromForm] string? content,
            [FromForm] int? parentMessageId,
            [FromForm] List<IFormFile> files)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                // Validate: must have at least one file
                if (files == null || files.Count == 0)
                    return BadRequest("At least one file is required.");

                // Validate each file
                var validationErrors = new List<string>();
                foreach (var file in files)
                {
                    if (file.Length == 0)
                    {
                        validationErrors.Add($"File '{file.FileName}' is empty.");
                        continue;
                    }

                    if (file.Length > _uploadSettings.MaxFileSizeBytes)
                    {
                        var maxMb = _uploadSettings.MaxFileSizeBytes / (1024 * 1024);
                        validationErrors.Add($"File '{file.FileName}' exceeds the maximum size of {maxMb} MB.");
                        continue;
                    }

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_uploadSettings.AllowedExtensions.Contains(ext))
                    {
                        validationErrors.Add($"File type '{ext}' is not allowed for '{file.FileName}'.");
                    }
                }

                if (validationErrors.Any())
                    return BadRequest(string.Join(" ", validationErrors));

                // Save files to storage
                var savedAttachments = new List<ChatMessageAttachment>();
                var savedUrls = new List<string>();

                try
                {
                    foreach (var file in files)
                    {
                        var fileUrl = await _fileStorage.SaveFileAsync(file, $"room_{roomId}");
                        savedUrls.Add(fileUrl);

                        var attachment = new ChatMessageAttachment
                        {
                            FileName = SanitizeFileName(file.FileName),
                            FileUrl = fileUrl,
                            FileType = file.ContentType ?? "application/octet-stream",
                            FileSize = file.Length
                        };

                        savedAttachments.Add(attachment);
                    }
                }
                catch (Exception ex)
                {
                    // Rollback: delete any files that were already saved
                    foreach (var url in savedUrls)
                    {
                        _fileStorage.DeleteFile(url);
                    }

                    _logger.LogError(ex, "Failed to save uploaded files for room {RoomId}", roomId);
                    return StatusCode(500, "Failed to upload files. Please try again.");
                }

                // Determine message type
                var allImages = savedAttachments.All(a => FileUploadSettings.IsImageMimeType(a.FileType));
                var messageType = allImages ? MessageType.Image : MessageType.File;

                // Build the message content (use provided content or auto-generate)
                var messageContent = string.IsNullOrWhiteSpace(content)
                    ? (savedAttachments.Count == 1
                        ? $"📎 {savedAttachments[0].FileName}"
                        : $"📎 {savedAttachments.Count} files")
                    : content.Trim();

                var request = new SendMessageRequest
                {
                    RoomId = roomId,
                    Content = messageContent,
                    MessageType = messageType,
                    ParentMessageId = parentMessageId
                };

                var message = await _chatRepository.SendMessageWithAttachmentsAsync(request, userId, savedAttachments);

                // Broadcast to all participants except sender
                var participants = await _chatRepository.GetRoomParticipantsAsync(message.RoomId);
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                if (otherUserIds.Any())
                    await _chatHub.Clients.Users(otherUserIds).SendAsync("ReceiveMessage", message);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SendMessageWithAttachments");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Download a file attachment by its ID.
        /// </summary>
        [HttpGet("attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var attachment = await _chatRepository.GetAttachmentByIdAsync(attachmentId);
                if (attachment == null)
                    return NotFound("Attachment not found.");

                // Verify user has access to the room
                var roomId = await _chatRepository.GetMessageRoomIdAsync(attachment.MessageId);
                if (!roomId.HasValue || !await _chatRepository.IsUserInRoomAsync(roomId.Value, userId))
                    return Forbid();

                var absolutePath = _fileStorage.GetAbsolutePath(attachment.FileUrl);
                if (!System.IO.File.Exists(absolutePath))
                    return NotFound("File not found on server.");

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(attachment.FileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                return File(fileStream, contentType, attachment.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
                return StatusCode(500, "Failed to download file.");
            }
        }

        /// <summary>
        /// Sanitize file name to prevent path traversal and invalid characters.
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unnamed_file";

            // Get just the file name (no directory components)
            var name = Path.GetFileName(fileName);

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                name = name.Replace(c, '_');
            }

            // Limit length
            if (name.Length > 200)
            {
                var ext = Path.GetExtension(name);
                name = name[..(200 - ext.Length)] + ext;
            }

            return string.IsNullOrWhiteSpace(name) ? "unnamed_file" : name;
        }

        // DELETE: api/chat/messages/{id}
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = GetCurrentUserId();
            var roomId = await _chatRepository.GetMessageRoomIdAsync(id);

            // Verify room access
            if (!roomId.HasValue || !await _chatRepository.IsUserInRoomAsync(roomId.Value, userId))
                return Forbid();

            var success = await _chatRepository.DeleteMessageAsync(id, userId);

            if (!success)
                return NotFound();

            // Broadcast to all participants except sender (sender updates local state)
            if (roomId.HasValue)
            {
                var participants = await _chatRepository.GetRoomParticipantsAsync(roomId.Value);
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                if (otherUserIds.Any())
                    await _chatHub.Clients.Users(otherUserIds).SendAsync("MessageDeleted", id);
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
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                if (otherUserIds.Any())
                    await _chatHub.Clients.Users(otherUserIds).SendAsync("MessageUpdated", message);
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
            var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
            if (otherUserIds.Any())
                await _chatHub.Clients.Users(otherUserIds).SendAsync("ReactionAdded", request.MessageId, userId, userName, request.Emoji);

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
            var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
            if (otherUserIds.Any())
                await _chatHub.Clients.Users(otherUserIds).SendAsync("ReactionRemoved", messageId, userId, emoji);

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

        #region Pinned Messages

        // POST: api/chat/rooms/{roomId}/pin
        [HttpPost("rooms/{roomId}/pin")]
        public async Task<ActionResult<ChatPinnedMessageResponse>> PinMessage(int roomId, [FromBody] PinMessageRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                var pinnedMessage = await _chatRepository.PinMessageAsync(roomId, request.MessageId, userId, request.Note);

                // Broadcast to all participants except sender
                var participants = await _chatRepository.GetRoomParticipantsAsync(roomId);
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                if (otherUserIds.Any())
                    await _chatHub.Clients.Users(otherUserIds).SendAsync("MessagePinned", pinnedMessage);

                return Ok(pinnedMessage);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning message {MessageId} in room {RoomId}", request.MessageId, roomId);
                return StatusCode(500, new { message = "An error occurred while pinning the message." });
            }
        }

        // DELETE: api/chat/rooms/{roomId}/pin/{messageId}
        [HttpDelete("rooms/{roomId}/pin/{messageId}")]
        public async Task<IActionResult> UnpinMessage(int roomId, int messageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                var success = await _chatRepository.UnpinMessageAsync(roomId, messageId, userId);
                if (!success)
                    return NotFound(new { message = "Pinned message not found." });

                // Broadcast to all participants except sender
                var participants = await _chatRepository.GetRoomParticipantsAsync(roomId);
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                if (otherUserIds.Any())
                    await _chatHub.Clients.Users(otherUserIds).SendAsync("MessageUnpinned", roomId, messageId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpinning message {MessageId} in room {RoomId}", messageId, roomId);
                return StatusCode(500, new { message = "An error occurred while unpinning the message." });
            }
        }

        // GET: api/chat/rooms/{roomId}/pinned
        [HttpGet("rooms/{roomId}/pinned")]
        public async Task<ActionResult<List<ChatPinnedMessageResponse>>> GetPinnedMessages(int roomId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!await _chatRepository.IsUserInRoomAsync(roomId, userId))
                    return Forbid();

                var pinnedMessages = await _chatRepository.GetPinnedMessagesAsync(roomId);
                return Ok(pinnedMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pinned messages for room {RoomId}", roomId);
                return StatusCode(500, new { message = "An error occurred while getting pinned messages." });
            }
        }

        #endregion

    }
}
