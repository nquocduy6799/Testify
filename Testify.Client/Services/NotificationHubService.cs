using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Client.Services
{
    public class NotificationHubService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        private readonly HashSet<string> _onlineUsers = new();

        public event Action<NotificationResponse>? OnNotificationReceived;
        public event Action? OnReconnected;
        public event Action<string, bool>? OnUserStatusChanged;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public IReadOnlySet<string> OnlineUsers => _onlineUsers;

        public NotificationHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartAsync()
        {
            if (_hubConnection != null)
            {
                Console.WriteLine("[NotificationHubService] Already connected or connecting");
                return;
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

        public async Task StopAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
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
