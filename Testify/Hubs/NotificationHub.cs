using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Testify.Hubs
{
    public class NotificationHub : Hub
    {
        // Track online users: UserId → List of ConnectionIds
        private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal notification group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Track user as online
                OnlineUsers.AddOrUpdate(
                    userId,
                    new HashSet<string> { Context.ConnectionId },
                    (key, existing) => { existing.Add(Context.ConnectionId); return existing; }
                );

                // Broadcast user online status to all clients
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
                
                Console.WriteLine($"[NotificationHub] User {userId} connected. Total connections: {OnlineUsers[userId].Count}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Remove connection from user's set
                if (OnlineUsers.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    
                    // If no more connections, user is offline
                    if (connections.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                        
                        // Broadcast user offline status
                        await Clients.All.SendAsync("UserStatusChanged", userId, false);
                        Console.WriteLine($"[NotificationHub] User {userId} went OFFLINE");
                    }
                    else
                    {
                        Console.WriteLine($"[NotificationHub] User {userId} disconnected. Remaining connections: {connections.Count}");
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Get list of online user IDs
        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(OnlineUsers.Keys.ToList());
        }

        // Check if specific user is online
        public Task<bool> IsUserOnline(string userId)
        {
            return Task.FromResult(OnlineUsers.ContainsKey(userId));
        }

        // Client can call this to test connection
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", "Connection is alive");
        }
    }
}
