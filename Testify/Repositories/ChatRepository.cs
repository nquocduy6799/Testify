using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.Chat;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPresenceService _presence;

        public ChatRepository(ApplicationDbContext context, IUserPresenceService presence)
        {
            _context = context;
            _presence = presence;
        }

        #region Rooms

        public async Task<List<ChatRoomResponse>> GetUserChatRoomsAsync(string userId)
        {
            var rooms = await _context.Set<ChatRoom>()
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .Include(r => r.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                    .ThenInclude(m => m.User)
                .Include(r => r.Project)
                .Where(r => !r.IsArchived && r.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(r => r.LastActivityAt)
                .AsSplitQuery()
                .ToListAsync();

            // Batch compute unread counts to avoid N+1 queries
            var roomIds = rooms.Select(r => r.Id).ToList();
            var unreadCounts = await BatchGetUnreadCountsAsync(roomIds, userId);

            var response = new List<ChatRoomResponse>();

            foreach (var room in rooms)
            {
                var roomDto = await MapToChatRoomResponse(room, userId, unreadCounts.GetValueOrDefault(room.Id));
                response.Add(roomDto);
            }

            return response;
        }

        public async Task<ChatRoomResponse?> GetChatRoomByIdAsync(int roomId)
        {
            var room = await _context.Set<ChatRoom>()
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsArchived);

            if (room == null) return null;

            return await MapToChatRoomResponse(room, null);
        }

        public async Task<bool> IsUserInRoomAsync(int roomId, string userId)
        {
            return await _context.Set<ChatRoomParticipant>()
                .AnyAsync(p => p.RoomId == roomId && p.UserId == userId);
        }

        public async Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request, string currentUserId)
        {
            var room = new ChatRoom
            {
                RoomName = request.RoomName,
                RoomType = request.RoomType,
                ProjectId = request.ProjectId,
                CreatedByUserId = currentUserId,
                LastActivityAt = DateTimeHelper.GetVietnamTime()
            };

            room.MarkAsCreated(currentUserId);

            _context.Set<ChatRoom>().Add(room);
            await _context.SaveChangesAsync();

            // Add creator as participant
            var creatorParticipant = new ChatRoomParticipant
            {
                RoomId = room.Id,
                UserId = currentUserId,
                Role = ChatParticipantRole.Owner,
                JoinedAt = DateTimeHelper.GetVietnamTime()
            };
            _context.Set<ChatRoomParticipant>().Add(creatorParticipant);

            // Add other participants
            if (request.RoomType == ChatRoomType.Private && !string.IsNullOrEmpty(request.OtherUserId))
            {
                var otherParticipant = new ChatRoomParticipant
                {
                    RoomId = room.Id,
                    UserId = request.OtherUserId,
                    Role = ChatParticipantRole.Member,
                    JoinedAt = DateTimeHelper.GetVietnamTime()
                };
                _context.Set<ChatRoomParticipant>().Add(otherParticipant);
            }
            else if (request.ParticipantUserIds.Any())
            {
                foreach (var participantId in request.ParticipantUserIds)
                {
                    if (participantId != currentUserId)
                    {
                        var participant = new ChatRoomParticipant
                        {
                            RoomId = room.Id,
                            UserId = participantId,
                            Role = ChatParticipantRole.Member,
                            JoinedAt = DateTimeHelper.GetVietnamTime()
                        };
                        _context.Set<ChatRoomParticipant>().Add(participant);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return await MapToChatRoomResponse(room, currentUserId);
        }

        public async Task<ChatRoomResponse?> GetOrCreateDirectRoomAsync(string userId1, string userId2)
        {
            // Check if room already exists
            var existingRoom = await _context.Set<ChatRoom>()
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .Where(r => r.RoomType == ChatRoomType.Private && !r.IsArchived)
                .Where(r => r.Participants.Count == 2 &&
                           r.Participants.Any(p => p.UserId == userId1) &&
                           r.Participants.Any(p => p.UserId == userId2))
                .FirstOrDefaultAsync();

            if (existingRoom != null)
            {
                return await MapToChatRoomResponse(existingRoom, userId1);
            }

            // Create new room
            var request = new CreateChatRoomRequest
            {
                RoomType = ChatRoomType.Private,
                OtherUserId = userId2
            };

            return await CreateChatRoomAsync(request, userId1);
        }

        public async Task<ChatRoomResponse?> UpdateRoomAsync(int roomId, UpdateRoomRequest request, string currentUserId)
        {
            var room = await _context.Set<ChatRoom>()
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return null;

            // Only group/project rooms can be renamed
            if (room.RoomType == ChatRoomType.Private) return null;

            if (!string.IsNullOrWhiteSpace(request.RoomName))
            {
                room.RoomName = request.RoomName.Trim();
            }

            room.UpdatedAt = DateTimeHelper.GetVietnamTime();
            room.UpdatedBy = currentUserId;
            await _context.SaveChangesAsync();

            return await GetChatRoomByIdAsync(roomId);
        }

        public async Task<bool> LeaveRoomAsync(int roomId, string userId)
        {
            var room = await _context.Set<ChatRoom>()
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return false;

            // Can't leave a private (1-1) room
            if (room.RoomType == ChatRoomType.Private) return false;

            var participant = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null) return false;

            _context.Set<ChatRoomParticipant>().Remove(participant);

            // If user was owner, transfer ownership to next admin or oldest member
            if (participant.Role == ChatParticipantRole.Owner)
            {
                var remaining = room.Participants.Where(p => p.UserId != userId).OrderBy(p => p.JoinedAt).ToList();
                var nextOwner = remaining.FirstOrDefault(p => p.Role == ChatParticipantRole.Admin) ?? remaining.FirstOrDefault();
                if (nextOwner != null)
                {
                    nextOwner.Role = ChatParticipantRole.Owner;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleMuteAsync(int roomId, string userId)
        {
            var participant = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
            if (participant == null) return false;

            participant.IsMuted = !participant.IsMuted;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatParticipantRole?> GetUserRoleInRoomAsync(int roomId, string userId)
        {
            var participant = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
            return participant?.Role;
        }

        #endregion

        #region Messages

        public async Task<List<ChatMessageResponse>> GetRoomMessagesAsync(int roomId, int skip = 0, int take = 50)
        {
            var messages = await _context.Set<ChatMessage>()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.Reactions)
                    .ThenInclude(r => r.User)
                .Include(m => m.Reads)
                    .ThenInclude(r => r.User)
                .Include(m => m.ParentMessage)
                    .ThenInclude(pm => pm.User)
                .Include(m => m.PinnedInRooms)
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .AsSplitQuery()
                .ToListAsync();

            return messages.Select(MapToMessageResponse).Reverse().ToList();
        }

        public async Task<(List<ChatMessageResponse> Messages, int TotalCount)> SearchMessagesAsync(int roomId, string query, int skip = 0, int take = 20)
        {
            var normalizedQuery = query.Trim().ToLower();

            var baseQuery = _context.Set<ChatMessage>()
                .Where(m => m.RoomId == roomId && !m.IsDeleted)
                .Where(m => EF.Functions.Like(m.Content.ToLower(), $"%{normalizedQuery}%")
                          || m.Attachments.Any(a => EF.Functions.Like(a.FileName.ToLower(), $"%{normalizedQuery}%")));

            var totalCount = await baseQuery.CountAsync();

            var messages = await baseQuery
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.Reactions)
                    .ThenInclude(r => r.User)
                .Include(m => m.Reads)
                    .ThenInclude(r => r.User)
                .Include(m => m.ParentMessage)
                    .ThenInclude(pm => pm!.User)
                .Include(m => m.PinnedInRooms)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .AsSplitQuery()
                .ToListAsync();

            return (messages.Select(MapToMessageResponse).ToList(), totalCount);
        }

        public async Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request, string currentUserId)
        {
            var message = new ChatMessage
            {
                RoomId = request.RoomId,
                UserId = currentUserId,
                MessageType = request.MessageType,
                Content = request.Content,
                ParentMessageId = request.ParentMessageId
            };

            message.MarkAsCreated(currentUserId);

            _context.Set<ChatMessage>().Add(message);

            // Update room's last activity
            var room = await _context.Set<ChatRoom>().FindAsync(request.RoomId);
            if (room != null)
            {
                room.LastActivityAt = DateTimeHelper.GetVietnamTime();
            }

            await _context.SaveChangesAsync();

            // Reload with includes
            var savedMessage = await _context.Set<ChatMessage>()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.Reactions)
                .Include(m => m.Reads)
                .Include(m => m.ParentMessage)
                    .ThenInclude(pm => pm.User)
                .AsSplitQuery()
                .FirstAsync(m => m.Id == message.Id);

            return MapToMessageResponse(savedMessage);
        }

        public async Task<int?> GetMessageRoomIdAsync(int messageId)
        {
            var message = await _context.Set<ChatMessage>().FindAsync(messageId);
            return message?.RoomId;
        }

        public async Task<ChatMessageResponse> SendMessageWithAttachmentsAsync(SendMessageRequest request, string currentUserId, List<ChatMessageAttachment> attachments)
        {
            var message = new ChatMessage
            {
                RoomId = request.RoomId,
                UserId = currentUserId,
                MessageType = request.MessageType,
                Content = request.Content,
                ParentMessageId = request.ParentMessageId
            };

            message.MarkAsCreated(currentUserId);

            _context.Set<ChatMessage>().Add(message);
            await _context.SaveChangesAsync();

            // Link attachments to the saved message
            foreach (var attachment in attachments)
            {
                attachment.MessageId = message.Id;
                _context.Set<ChatMessageAttachment>().Add(attachment);
            }

            // Update room's last activity
            var room = await _context.Set<ChatRoom>().FindAsync(request.RoomId);
            if (room != null)
            {
                room.LastActivityAt = DateTimeHelper.GetVietnamTime();
            }

            await _context.SaveChangesAsync();

            // Reload with includes
            var savedMessage = await _context.Set<ChatMessage>()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.Reactions)
                .Include(m => m.Reads)
                .Include(m => m.ParentMessage)
                    .ThenInclude(pm => pm!.User)
                .AsSplitQuery()
                .FirstAsync(m => m.Id == message.Id);

            return MapToMessageResponse(savedMessage);
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string currentUserId)
        {
            var message = await _context.Set<ChatMessage>().FindAsync(messageId);
            
            if (message == null || message.UserId != currentUserId)
                return false;

            message.IsDeleted = true;
            message.Content = "The message has been withdrawn.";
            message.MarkAsUpdated(currentUserId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent, string currentUserId)
        {
            var message = await _context.Set<ChatMessage>()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.Reactions)
                    .ThenInclude(r => r.User)
                .Include(m => m.Reads)
                    .ThenInclude(r => r.User)
                .Include(m => m.ParentMessage)
                    .ThenInclude(pm => pm!.User)
                .Include(m => m.PinnedInRooms)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.UserId != currentUserId)
                throw new InvalidOperationException("Cannot update message");

            message.Content = newContent;
            message.MarkAsUpdated(currentUserId);

            await _context.SaveChangesAsync();

            return MapToMessageResponse(message);
        }

        public async Task<ChatMessageAttachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.Set<ChatMessageAttachment>()
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
        }

        #endregion

        #region Reactions

        public async Task<bool> AddReactionAsync(AddReactionRequest request, string currentUserId)
        {
            // Check if reaction already exists
            var existing = await _context.Set<ChatMessageReaction>()
                .FirstOrDefaultAsync(r => r.MessageId == request.MessageId && 
                                        r.UserId == currentUserId && 
                                        r.Reaction == request.Emoji);

            if (existing != null)
                return false; // Already reacted with this emoji

            var reaction = new ChatMessageReaction
            {
                MessageId = request.MessageId,
                UserId = currentUserId,
                Reaction = request.Emoji,
                CreatedAt = DateTimeHelper.GetVietnamTime()
            };

            _context.Set<ChatMessageReaction>().Add(reaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveReactionAsync(int messageId, string emoji, string currentUserId)
        {
            var reaction = await _context.Set<ChatMessageReaction>()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && 
                                        r.UserId == currentUserId && 
                                        r.Reaction == emoji);

            if (reaction == null)
                return false;

            _context.Set<ChatMessageReaction>().Remove(reaction);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Read Receipts

        public async Task<bool> MarkAsReadAsync(int roomId, int messageId, string currentUserId)
        {
            // Check if already read
            var existing = await _context.Set<ChatMessageRead>()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == currentUserId);

            if (existing != null)
                return false;

            var read = new ChatMessageRead
            {
                MessageId = messageId,
                UserId = currentUserId,
                ReadAt = DateTimeHelper.GetVietnamTime()
            };

            _context.Set<ChatMessageRead>().Add(read);

            // Update participant's last read
            var participant = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == currentUserId);

            if (participant != null)
            {
                // Only advance LastReadMessageId forward, never regress
                if (!participant.LastReadMessageId.HasValue || messageId > participant.LastReadMessageId.Value)
                {
                    participant.LastReadMessageId = messageId;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int roomId, string currentUserId)
        {
            var participant = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == currentUserId);

            if (participant == null || !participant.LastReadMessageId.HasValue)
            {
                // Count all messages
                return await _context.Set<ChatMessage>()
                    .CountAsync(m => m.RoomId == roomId && m.UserId != currentUserId && !m.IsDeleted);
            }

            // Count messages after last read
            return await _context.Set<ChatMessage>()
                .CountAsync(m => m.RoomId == roomId && 
                               m.Id > participant.LastReadMessageId.Value && 
                               m.UserId != currentUserId && 
                               !m.IsDeleted);
        }

        private async Task<Dictionary<int, int>> BatchGetUnreadCountsAsync(List<int> roomIds, string userId)
        {
            var participants = await _context.Set<ChatRoomParticipant>()
                .Where(p => p.UserId == userId && roomIds.Contains(p.RoomId))
                .Select(p => new { p.RoomId, p.LastReadMessageId })
                .ToListAsync();

            var result = roomIds.ToDictionary(id => id, _ => 0);

            var noLastReadIds = participants.Where(p => !p.LastReadMessageId.HasValue).Select(p => p.RoomId).ToList();
            if (noLastReadIds.Any())
            {
                var counts = await _context.Set<ChatMessage>()
                    .Where(m => noLastReadIds.Contains(m.RoomId) && m.UserId != userId && !m.IsDeleted)
                    .GroupBy(m => m.RoomId)
                    .Select(g => new { RoomId = g.Key, Count = g.Count() })
                    .ToListAsync();
                foreach (var c in counts) result[c.RoomId] = c.Count;
            }

            var withLastRead = participants.Where(p => p.LastReadMessageId.HasValue).ToList();
            if (withLastRead.Any())
            {
                var minId = withLastRead.Min(p => p.LastReadMessageId!.Value);
                var withLastReadRoomIds = withLastRead.Select(p => p.RoomId).ToList();
                var msgData = await _context.Set<ChatMessage>()
                    .Where(m => withLastReadRoomIds.Contains(m.RoomId) && m.Id > minId && m.UserId != userId && !m.IsDeleted)
                    .Select(m => new { m.RoomId, m.Id })
                    .ToListAsync();
                foreach (var p in withLastRead)
                {
                    result[p.RoomId] = msgData.Count(m => m.RoomId == p.RoomId && m.Id > p.LastReadMessageId!.Value);
                }
            }

            return result;
        }

        #endregion

        #region Participants

        public async Task<List<ChatParticipantResponse>> GetRoomParticipantsAsync(int roomId)
        {
            var participants = await _context.Set<ChatRoomParticipant>()
                .Include(p => p.User)
                .Where(p => p.RoomId == roomId)
                .ToListAsync();

            return participants.Select(p => new ChatParticipantResponse
            {
                Id = p.Id,
                RoomId = p.RoomId,
                UserId = p.UserId,
                UserName = p.User.UserName ?? "",
                FullName = p.User.FullName,
                AvatarUrl = p.User.AvatarUrl,
                JoinedAt = p.JoinedAt,
                Role = p.Role,
                IsMuted = p.IsMuted,
                IsPinned = p.IsPinned,
                LastReadMessageId = p.LastReadMessageId
            }).ToList();
        }

        public async Task<bool> AddParticipantAsync(int roomId, string userId, string addedBy)
        {
            var existing = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);

            if (existing != null)
                return false; // Already a participant

            var participant = new ChatRoomParticipant
            {
                RoomId = roomId,
                UserId = userId,
                Role = ChatParticipantRole.Member,
                JoinedAt = DateTimeHelper.GetVietnamTime()
            };

            _context.Set<ChatRoomParticipant>().Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveParticipantAsync(int roomId, string userId, string removedBy)
        {
            var participant = await _context.Set<ChatRoomParticipant>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);

            if (participant == null)
                return false;

            _context.Set<ChatRoomParticipant>().Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Mapping

        private async Task<ChatRoomResponse> MapToChatRoomResponse(ChatRoom room, string? currentUserId, int? precomputedUnreadCount = null)
        {
            var lastMessage = room.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            
            var response = new ChatRoomResponse
            {
                Id = room.Id,
                RoomName = room.RoomName,
                RoomType = room.RoomType,
                ProjectId = room.ProjectId,
                ProjectName = room.Project?.Name,
                LastActivityAt = room.LastActivityAt,
                IsArchived = room.IsArchived,
                CreatedAt = room.CreatedAt,
                CreatedBy = room.CreatedByUserId,
                LastMessage = lastMessage != null ? (lastMessage.IsDeleted ? "The message has been withdrawn." : lastMessage.Content) : null,
                LastMessageSender = lastMessage?.User?.UserName,
                LastMessageTime = lastMessage?.CreatedAt,
                Participants = room.Participants?.Select(p => new ChatParticipantResponse
                {
                    Id = p.Id,
                    RoomId = p.RoomId,
                    UserId = p.UserId,
                    UserName = p.User?.UserName ?? "",
                    FullName = p.User?.FullName,
                    AvatarUrl = p.User?.AvatarUrl,
                    JoinedAt = p.JoinedAt,
                    Role = p.Role,
                    IsMuted = p.IsMuted,
                    IsPinned = p.IsPinned,
                    LastReadMessageId = p.LastReadMessageId
                }).ToList() ?? new List<ChatParticipantResponse>()
            };

            // For direct messages, set OtherParticipant and online status
            if (room.RoomType == ChatRoomType.Private && !string.IsNullOrEmpty(currentUserId))
            {
                response.OtherParticipant = response.Participants
                    .FirstOrDefault(p => p.UserId != currentUserId);
                if (response.OtherParticipant != null)
                {
                    response.OtherParticipant.IsOnline = _presence.IsOnline(response.OtherParticipant.UserId);
                    response.OtherParticipant.Status = response.OtherParticipant.IsOnline ? "Online" : "Offline";
                }
            }

            // Get unread count if currentUserId provided
            if (!string.IsNullOrEmpty(currentUserId))
            {
                response.UnreadCount = precomputedUnreadCount ?? await GetUnreadCountAsync(room.Id, currentUserId);
            }

            return response;
        }

        private ChatMessageResponse MapToMessageResponse(ChatMessage message)
        {
            return new ChatMessageResponse
            {
                Id = message.Id,
                RoomId = message.RoomId,
                UserId = message.UserId,
                UserName = message.User?.UserName ?? "",
                UserAvatarUrl = message.User?.AvatarUrl,
                MessageType = message.MessageType,
                Content = message.Content,
                IsDeleted = message.IsDeleted,
                ParentMessageId = message.ParentMessageId,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                Attachments = message.Attachments?.Select(a => new ChatMessageAttachmentResponse
                {
                    Id = a.Id,
                    MessageId = a.MessageId,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt
                }).ToList() ?? new List<ChatMessageAttachmentResponse>(),
                Reactions = message.Reactions?.Select(r => new ChatMessageReactionResponse
                {
                    Id = r.Id,
                    MessageId = r.MessageId,
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "",
                    Emoji = r.Reaction,
                    CreatedAt = r.CreatedAt
                }).ToList() ?? new List<ChatMessageReactionResponse>(),
                ReadBy = message.Reads?.Select(r => new ChatMessageReadResponse
                {
                    Id = r.Id,
                    MessageId = r.MessageId,
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "",
                    ReadAt = r.ReadAt
                }).ToList() ?? new List<ChatMessageReadResponse>(),
                ParentMessage = message.ParentMessage != null ? new ChatMessageResponse
                {
                    Id = message.ParentMessage.Id,
                    UserId = message.ParentMessage.UserId,
                    UserName = message.ParentMessage.User?.UserName ?? "",
                    Content = message.ParentMessage.IsDeleted ? "[The message has been withdrawn.]" : message.ParentMessage.Content,
                    IsDeleted = message.ParentMessage.IsDeleted,
                    CreatedAt = message.ParentMessage.CreatedAt
                } : null,
                IsPinned = message.PinnedInRooms?.Any() ?? false,
                Metadata = message.Metadata
            };
        }

        #endregion

        #region Pinned Messages

        public async Task<ChatPinnedMessageResponse> PinMessageAsync(int roomId, int messageId, string userId, string? note)
        {
            // Check if already pinned
            var existing = await _context.Set<ChatPinnedMessage>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.MessageId == messageId);

            if (existing != null)
                throw new InvalidOperationException("Message is already pinned in this room.");

            var pinnedMessage = new ChatPinnedMessage
            {
                RoomId = roomId,
                MessageId = messageId,
                PinnedByUserId = userId,
                Note = note
            };

            _context.Set<ChatPinnedMessage>().Add(pinnedMessage);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var result = await _context.Set<ChatPinnedMessage>()
                .Include(p => p.PinnedBy)
                .Include(p => p.Message)
                    .ThenInclude(m => m.User)
                .Include(p => p.Message)
                    .ThenInclude(m => m.Attachments)
                .Include(p => p.Message)
                    .ThenInclude(m => m.Reactions)
                        .ThenInclude(r => r.User)
                .FirstAsync(p => p.Id == pinnedMessage.Id);

            return MapToPinnedMessageResponse(result);
        }

        public async Task<bool> UnpinMessageAsync(int roomId, int messageId, string userId)
        {
            var pinned = await _context.Set<ChatPinnedMessage>()
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.MessageId == messageId);

            if (pinned == null) return false;

            _context.Set<ChatPinnedMessage>().Remove(pinned);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChatPinnedMessageResponse>> GetPinnedMessagesAsync(int roomId)
        {
            var pinned = await _context.Set<ChatPinnedMessage>()
                .Include(p => p.PinnedBy)
                .Include(p => p.Message)
                    .ThenInclude(m => m.User)
                .Include(p => p.Message)
                    .ThenInclude(m => m.Attachments)
                .Include(p => p.Message)
                    .ThenInclude(m => m.Reactions)
                        .ThenInclude(r => r.User)
                .Where(p => p.RoomId == roomId)
                .OrderByDescending(p => p.PinnedAt)
                .AsSplitQuery()
                .ToListAsync();

            return pinned.Select(MapToPinnedMessageResponse).ToList();
        }

        public async Task<bool> IsMessagePinnedAsync(int roomId, int messageId)
        {
            return await _context.Set<ChatPinnedMessage>()
                .AnyAsync(p => p.RoomId == roomId && p.MessageId == messageId);
        }

        private ChatPinnedMessageResponse MapToPinnedMessageResponse(ChatPinnedMessage pin)
        {
            return new ChatPinnedMessageResponse
            {
                Id = pin.Id,
                RoomId = pin.RoomId,
                MessageId = pin.MessageId,
                PinnedByUserId = pin.PinnedByUserId,
                PinnedByUserName = pin.PinnedBy?.UserName ?? "",
                PinnedAt = pin.PinnedAt,
                Note = pin.Note,
                Message = MapToMessageResponse(pin.Message)
            };
        }

        #endregion
    }
}
