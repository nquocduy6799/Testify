using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testify.Shared.DTOs.Chat;

namespace Testify.Client.Features.Chat.Services
{
    public class ChatHubService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        private int? _currentRoomId;

        // Events
        public event Action<ChatMessageResponse>? OnMessageReceived;
        public event Action<int>? OnMessageDeleted;
        public event Action<ChatMessageResponse>? OnMessageUpdated;
        public event Action<string, string, bool>? OnUserTyping; // userId, userName, isTyping
        public event Action<string, string>? OnUserJoinedRoom; // userId, userName
        public event Action<string, string>? OnUserLeftRoom; // userId, userName
        public event Action<int, string, string, string>? OnReactionAdded; // messageId, userId, userName, emoji
        public event Action<int, string, string>? OnReactionRemoved; // messageId, userId, emoji
        public event Action<int, string, string>? OnMessageRead; // messageId, userId, userName
        public event Action<ChatPinnedMessageResponse>? OnMessagePinned; // pinned message response
        public event Action<int, int>? OnMessageUnpinned; // roomId, messageId
        public event Action<ChatRoomResponse>? OnRoomUpdated; // updated room
        public event Action<int, List<ChatParticipantResponse>>? OnParticipantsAdded; // roomId, added participants
        public event Action<int, string, string>? OnParticipantRemoved; // roomId, userId, userName

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public int? CurrentRoomId => _currentRoomId;

        public ChatHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartAsync()
        {
            // If already connected, do nothing
            if (_hubConnection?.State == HubConnectionState.Connected) return;

            // If connection exists but is disconnected/stopped, dispose and recreate
            if (_hubConnection != null)
            {
                try { await _hubConnection.DisposeAsync(); } catch { }
                _hubConnection = null;
            }

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/chat"))
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                    .Build();

                _hubConnection.On<ChatMessageResponse>("ReceiveMessage", msg => OnMessageReceived?.Invoke(msg));
                _hubConnection.On<int>("MessageDeleted", id => OnMessageDeleted?.Invoke(id));
                _hubConnection.On<ChatMessageResponse>("MessageUpdated", msg => OnMessageUpdated?.Invoke(msg));
                _hubConnection.On<string, string, bool>("UserTyping", (uid, name, typing) => OnUserTyping?.Invoke(uid, name, typing));
                _hubConnection.On<string, string>("UserJoinedRoom", (uid, name) => OnUserJoinedRoom?.Invoke(uid, name));
                _hubConnection.On<string, string>("UserLeftRoom", (uid, name) => OnUserLeftRoom?.Invoke(uid, name));
                _hubConnection.On<int, string, string, string>("ReactionAdded", (mid, uid, name, emoji) => OnReactionAdded?.Invoke(mid, uid, name, emoji));
                _hubConnection.On<int, string, string>("ReactionRemoved", (mid, uid, emoji) => OnReactionRemoved?.Invoke(mid, uid, emoji));
                _hubConnection.On<int, string, string>("MessageRead", (mid, uid, name) => OnMessageRead?.Invoke(mid, uid, name));
                _hubConnection.On<ChatPinnedMessageResponse>("MessagePinned", (pin) => OnMessagePinned?.Invoke(pin));
                _hubConnection.On<int, int>("MessageUnpinned", (roomId, messageId) => OnMessageUnpinned?.Invoke(roomId, messageId));
                _hubConnection.On<ChatRoomResponse>("RoomUpdated", (room) => OnRoomUpdated?.Invoke(room));
                _hubConnection.On<int, List<ChatParticipantResponse>>("ParticipantsAdded", (roomId, participants) => OnParticipantsAdded?.Invoke(roomId, participants));
                _hubConnection.On<int, string, string>("ParticipantRemoved", (roomId, userId, userName) => OnParticipantRemoved?.Invoke(roomId, userId, userName));

                _hubConnection.Reconnected += async _ =>
                {
                    if (_currentRoomId.HasValue)
                        await JoinRoomAsync(_currentRoomId.Value);
                };

                _hubConnection.Closed += async _ =>
                {
                    _hubConnection = null;
                    await Task.Delay(5000);
                    await StartAsync();
                };

                await _hubConnection.StartAsync();
            }
            catch (Exception)
            {
                _hubConnection = null;
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection == null) return;
            try
            {
                if (_currentRoomId.HasValue)
                    await LeaveRoomAsync(_currentRoomId.Value);
                await _hubConnection.StopAsync();
            }
            finally
            {
                _hubConnection = null;
            }
        }

        #region Room Management

        public async Task JoinRoomAsync(int roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;

            try
            {
                if (_currentRoomId.HasValue && _currentRoomId.Value != roomId)
                    await LeaveRoomAsync(_currentRoomId.Value);
                await _hubConnection.InvokeAsync("JoinRoom", roomId);
                _currentRoomId = roomId;
            }
            catch { _currentRoomId = null; }
        }

        public async Task LeaveRoomAsync(int roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try
            {
                await _hubConnection.InvokeAsync("LeaveRoom", roomId);
                if (_currentRoomId == roomId) _currentRoomId = null;
            }
            catch { }
        }

        #endregion

        #region Typing Indicators

        public async Task StartTypingAsync(int roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("StartTyping", roomId); } catch { }
        }

        public async Task StopTypingAsync(int roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("StopTyping", roomId); } catch { }
        }

        public async Task<List<string>> GetTypingUsersAsync(int roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
                return new List<string>();
            try { return await _hubConnection.InvokeAsync<List<string>>("GetTypingUsers", roomId); }
            catch { return new List<string>(); }
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    if (_currentRoomId.HasValue && _hubConnection.State == HubConnectionState.Connected)
                        await _hubConnection.InvokeAsync("LeaveRoom", _currentRoomId.Value);
                }
                catch { }
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _currentRoomId = null;
            }
        }
    }
}
