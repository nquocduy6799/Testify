using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Testify.Interfaces;

namespace Testify.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserPresenceService _presence;

        // Track which users are in which rooms (thread-safe)
        // Key: RoomId, Value: Set of ConnectionIds
        // TODO: For multi-instance deployment, replace static state with Redis backplane
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> RoomConnections = new();
        
        // Track user typing status
        // Key: RoomId, Value: Dictionary<UserId, Timestamp>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, DateTime>> TypingUsers = new();

        public ChatHub(IUserPresenceService presence)
        {
            _presence = presence;
        }

        private string GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? throw new InvalidOperationException("User not authenticated");
        }

        private string GetCurrentUserName()
        {
            return Context.User?.Identity?.Name ?? "Unknown";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            _presence.AddConnection(userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            _presence.RemoveConnection(userId, Context.ConnectionId);

            // Cleanup typing indicators for disconnected user
            foreach (var roomId in TypingUsers.Keys.ToList())
            {
                if (TypingUsers.TryGetValue(roomId, out var typingDict) && typingDict.TryRemove(userId, out _))
                {
                    await Clients.Group($"room_{roomId}").SendAsync("UserTyping", userId, userName, false);
                }
            }

            foreach (var roomId in RoomConnections.Keys.ToList())
            {
                if (!RoomConnections.TryGetValue(roomId, out var connections))
                    continue;
                connections.TryRemove(Context.ConnectionId, out _);
                if (connections.IsEmpty)
                    RoomConnections.TryRemove(roomId, out _);
                await Clients.Group($"room_{roomId}").SendAsync("UserLeftRoom", userId, userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        #region Room Management

        /// <summary>
        /// Join a chat room to receive messages
        /// </summary>
        public async Task JoinRoom(int roomId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
            
            // Track connection (thread-safe)
            RoomConnections.AddOrUpdate(
                roomId,
                _ => { var d = new ConcurrentDictionary<string, byte>(); d.TryAdd(Context.ConnectionId, 0); return d; },
                (_, existing) => { existing.TryAdd(Context.ConnectionId, 0); return existing; }
            );

            await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserJoinedRoom", userId, userName);
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        public async Task LeaveRoom(int roomId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
            
            // Remove from tracking
            if (RoomConnections.TryGetValue(roomId, out var connections))
            {
                connections.TryRemove(Context.ConnectionId, out _);
                if (connections.IsEmpty)
                {
                    RoomConnections.TryRemove(roomId, out _);
                }
            }

            await Clients.Group($"room_{roomId}").SendAsync("UserLeftRoom", userId, userName);
        }

        #endregion

        #region Typing Indicators

        /// <summary>
        /// User started typing
        /// </summary>
        public async Task StartTyping(int roomId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            
            // Track typing
            TypingUsers.AddOrUpdate(
                roomId,
                new ConcurrentDictionary<string, DateTime> { [userId] = DateTime.UtcNow },
                (key, existing) => { existing[userId] = DateTime.UtcNow; return existing; }
            );

            // Notify others (not self)
            await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", userId, userName, true);
        }

        /// <summary>
        /// User stopped typing
        /// </summary>
        public async Task StopTyping(int roomId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            
            // Remove from typing
            if (TypingUsers.TryGetValue(roomId, out var users))
            {
                users.TryRemove(userId, out _);
            }

            // Notify others
            await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", userId, userName, false);
        }

        /// <summary>
        /// Get currently typing users in room
        /// </summary>
        public Task<List<string>> GetTypingUsers(int roomId)
        {
            if (!TypingUsers.TryGetValue(roomId, out var users))
                return Task.FromResult(new List<string>());

            // Remove stale typing indicators (>5 seconds)
            var staleUsers = users.Where(u => (DateTime.UtcNow - u.Value).TotalSeconds > 5)
                                 .Select(u => u.Key)
                                 .ToList();

            foreach (var staleUser in staleUsers)
            {
                users.TryRemove(staleUser, out _);
            }

            return Task.FromResult(users.Keys.ToList());
        }

        #endregion
    }
}
