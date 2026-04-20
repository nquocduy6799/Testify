using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Testify.Shared.DTOs.Meetings;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Client.Shared.Services
{
    public class NotificationHubService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        private readonly HashSet<string> _onlineUsers = new();

        public event Action<NotificationResponse>? OnNotificationReceived;
        public event Action? OnReconnected;
        public event Action<string, bool>? OnUserStatusChanged;
        public event Action<int, string, string>? OnTeamMemberAdded; // (projectId, userId, userName)
        public event Action<int, string>? OnTeamMemberRemoved; // (projectId, userId)
        public event Action<int>? OnMilestoneCreated; // (projectId)
        public event Action<int>? OnMilestoneUpdated; // (projectId)
        public event Action<int, int>? OnMilestoneDeleted; // (projectId, milestoneId)
        public event Action<MeetingResponse>? OnMeetingStarted;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public IReadOnlySet<string> OnlineUsers => _onlineUsers;

        public NotificationHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartAsync()
        {
            // If already connected, do nothing
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                Console.WriteLine("[NotificationHubService] Already connected");
                return;
            }

            // If connection exists but is disconnected/stopped, dispose it and create a new one
            if (_hubConnection != null)
            {
                Console.WriteLine("[NotificationHubService] Disposing stale connection before reconnecting");
                try { await _hubConnection.DisposeAsync(); } catch { }
                _hubConnection = null;
            }

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/notifications"))
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                    .Build();

                // Listen for notifications
                _hubConnection.On<NotificationResponse>("ReceiveNotification", (notification) =>
                {
                    Console.WriteLine($"[SignalR] Received notification: {notification.Title}");
                    OnNotificationReceived?.Invoke(notification);
                });

                // Listen for user status changes
                _hubConnection.On<string, bool>("UserStatusChanged", (userId, isOnline) =>
                {
                    Console.WriteLine($"[SignalR] User {userId} is now {(isOnline ? "ONLINE" : "OFFLINE")}");
                    
                    if (isOnline)
                        _onlineUsers.Add(userId);
                    else
                        _onlineUsers.Remove(userId);
                    
                    OnUserStatusChanged?.Invoke(userId, isOnline);
                });

                // Listen for team member added
                _hubConnection.On<int, string, string>("TeamMemberAdded", (projectId, userId, userName) =>
                {
                    Console.WriteLine($"[SignalR] TeamMemberAdded - Project {projectId}, User {userName}");
                    OnTeamMemberAdded?.Invoke(projectId, userId, userName);
                });

                // Listen for team member removed
                _hubConnection.On<int, string>("TeamMemberRemoved", (projectId, userId) =>
                {
                    Console.WriteLine($"[SignalR] TeamMemberRemoved - Project {projectId}, User {userId}");
                    OnTeamMemberRemoved?.Invoke(projectId, userId);
                });

                // Listen for milestone created
                _hubConnection.On<int, object>("MilestoneCreated", (projectId, milestone) =>
                {
                    Console.WriteLine($"[SignalR] MilestoneCreated - Project {projectId}");
                    OnMilestoneCreated?.Invoke(projectId);
                });

                // Listen for milestone updated
                _hubConnection.On<int, object>("MilestoneUpdated", (projectId, milestone) =>
                {
                    Console.WriteLine($"[SignalR] MilestoneUpdated - Project {projectId}");
                    OnMilestoneUpdated?.Invoke(projectId);
                });

                // Listen for milestone deleted
                _hubConnection.On<int, int>("MilestoneDeleted", (projectId, milestoneId) =>
                {
                    Console.WriteLine($"[SignalR] MilestoneDeleted - Project {projectId}, Milestone {milestoneId}");
                    OnMilestoneDeleted?.Invoke(projectId, milestoneId);
                });

                // Listen for meeting started (broadcast to all project members)
                _hubConnection.On<MeetingResponse>("MeetingStarted", meeting =>
                {
                    Console.WriteLine($"[SignalR] MeetingStarted - Meeting {meeting.Id}, Project {meeting.ProjectId}");
                    OnMeetingStarted?.Invoke(meeting);
                });

                // Reconnect event
                _hubConnection.Reconnected += async (connectionId) =>
                {
                    Console.WriteLine($"[SignalR] Reconnected with ConnectionId: {connectionId}");
                    OnReconnected?.Invoke();
                    await Task.CompletedTask;
                };

                _hubConnection.Closed += async (error) =>
                {
                    Console.WriteLine($"[SignalR] Connection closed: {error?.Message}");
                    // Reset connection so StartAsync can create a new one
                    _hubConnection = null;
                    // Attempt to reconnect after 5 seconds
                    await Task.Delay(5000);
                    await StartAsync();
                };

                await _hubConnection.StartAsync();
                Console.WriteLine($"[SignalR] Connected with ConnectionId: {_hubConnection.ConnectionId}");
                
                // Get initial list of online users
                try
                {
                    var onlineUsers = await _hubConnection.InvokeAsync<List<string>>("GetOnlineUsers");
                    foreach (var userId in onlineUsers)
                    {
                        _onlineUsers.Add(userId);
                    }
                    Console.WriteLine($"[SignalR] Loaded {onlineUsers.Count} online users");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Error loading online users: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Error connecting: {ex.Message}");
            }
        }

        // ============================================
        // PROJECT GROUP MANAGEMENT
        // ============================================

        public async Task JoinProjectGroupAsync(int projectId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinProjectGroup", projectId);
                    Console.WriteLine($"[SignalR] Joined project group: project_{projectId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Error joining project group {projectId}: {ex.Message}");
                }
            }
        }

        public async Task LeaveProjectGroupAsync(int projectId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("LeaveProjectGroup", projectId);
                    Console.WriteLine($"[SignalR] Left project group: project_{projectId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Error leaving project group {projectId}: {ex.Message}");
                }
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection != null)
            {
                try { await _hubConnection.StopAsync(); } catch { }
                try { await _hubConnection.DisposeAsync(); } catch { }
                _hubConnection = null;
                _onlineUsers.Clear();
                Console.WriteLine("[SignalR] Disconnected");
            }
        }

        public bool IsUserOnline(string userId)
        {
            return _onlineUsers.Contains(userId);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
