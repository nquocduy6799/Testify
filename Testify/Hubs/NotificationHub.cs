using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Testify.Interfaces;

namespace Testify.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IUserPresenceService _presence;

        public NotificationHub(IUserPresenceService presence)
        {
            _presence = presence;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _presence.AddConnection(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
                Console.WriteLine($"[NotificationHub] User {userId} connected.");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _presence.RemoveConnection(userId, Context.ConnectionId);
                if (!_presence.IsOnline(userId))
                {
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
                    Console.WriteLine($"[NotificationHub] User {userId} went OFFLINE");
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(_presence.GetOnlineUserIds().ToList());
        }

        public Task<bool> IsUserOnline(string userId)
        {
            return Task.FromResult(_presence.IsOnline(userId));
        }

        // Broadcast team member added event
        public async Task BroadcastTeamMemberAdded(int projectId, string userId, string userName)
        {
            await Clients.All.SendAsync("TeamMemberAdded", projectId, userId, userName);
            Console.WriteLine($"[NotificationHub] Broadcast: TeamMemberAdded - Project {projectId}, User {userName}");
        }

        // Broadcast team member removed event
        public async Task BroadcastTeamMemberRemoved(int projectId, string userId)
        {
            await Clients.All.SendAsync("TeamMemberRemoved", projectId, userId);
            Console.WriteLine($"[NotificationHub] Broadcast: TeamMemberRemoved - Project {projectId}, User {userId}");
        }

        // ============================================
        // MILESTONE EVENTS
        // ============================================
        
        // Broadcast milestone created
        public async Task BroadcastMilestoneCreated(int projectId, object milestone)
        {
            await Clients.All.SendAsync("MilestoneCreated", projectId, milestone);
            Console.WriteLine($"[NotificationHub] Broadcast: MilestoneCreated - Project {projectId}");
        }

        // Broadcast milestone updated
        public async Task BroadcastMilestoneUpdated(int projectId, object milestone)
        {
            await Clients.All.SendAsync("MilestoneUpdated", projectId, milestone);
            Console.WriteLine($"[NotificationHub] Broadcast: MilestoneUpdated - Project {projectId}");
        }

        // Broadcast milestone deleted
        public async Task BroadcastMilestoneDeleted(int projectId, int milestoneId)
        {
            await Clients.All.SendAsync("MilestoneDeleted", projectId, milestoneId);
            Console.WriteLine($"[NotificationHub] Broadcast: MilestoneDeleted - Project {projectId}, Milestone {milestoneId}");
        }

        // Client can call this to test connection
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", "Connection is alive");
        }
    }
}
