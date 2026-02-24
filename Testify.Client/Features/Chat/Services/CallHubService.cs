using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Testify.Shared.DTOs.Chat;

namespace Testify.Client.Features.Chat.Services
{
    public class CallHubService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;

        // Events for call signaling
        public event Action<IncomingCallResponse>? OnIncomingCall;
        public event Action<int>? OnCallRinging; // callSessionId
        public event Action<CallAnsweredResponse>? OnCallAnswered;
        public event Action<IceCandidateRequest>? OnIceCandidateReceived;
        public event Action<CallEndedResponse>? OnCallEnded;
        public event Action<string>? OnCallError;
        public event Action<string, bool, bool>? OnMediaToggled; // userId, isAudioEnabled, isVideoEnabled
        public event Func<Task>? OnReconnected;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        /// <summary>
        /// Stores incoming call data when accepted from a non-Messages page,
        /// so the Messages page can pick it up and complete the WebRTC handshake.
        /// </summary>
        public IncomingCallResponse? PendingIncomingCall { get; set; }

        public CallHubService(NavigationManager navigationManager)
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
                    .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/call"))
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                    .Build();

                // Register event handlers
                _hubConnection.On<IncomingCallResponse>("IncomingCall", call => OnIncomingCall?.Invoke(call));
                _hubConnection.On<int>("CallRinging", id => OnCallRinging?.Invoke(id));
                _hubConnection.On<CallAnsweredResponse>("CallAnswered", resp => OnCallAnswered?.Invoke(resp));
                _hubConnection.On<IceCandidateRequest>("IceCandidateReceived", ice => OnIceCandidateReceived?.Invoke(ice));
                _hubConnection.On<CallEndedResponse>("CallEnded", resp => OnCallEnded?.Invoke(resp));
                _hubConnection.On<string>("CallError", msg => OnCallError?.Invoke(msg));
                _hubConnection.On<string, bool, bool>("MediaToggled", (uid, audio, video) => OnMediaToggled?.Invoke(uid, audio, video));

                // WithAutomaticReconnect handles reconnection. Only use Closed for terminal failures.
                _hubConnection.Closed += async _ =>
                {
                    Console.WriteLine("[CallHub] Connection closed permanently. Will retry in 5s.");
                    _hubConnection = null;
                    await Task.Delay(5000);
                    try { await StartAsync(); } catch { }
                };

                _hubConnection.Reconnected += async _ =>
                {
                    Console.WriteLine("[CallHub] Reconnected.");
                    if (OnReconnected != null)
                        await OnReconnected.Invoke();
                };

                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CallHub] Failed to start: {ex.Message}");
                _hubConnection = null;
            }
        }

        public async Task SendOfferAsync(CallOfferRequest request)
        {
            await InvokeHubMethodAsync("SendOffer", request);
        }

        public async Task SendAnswerAsync(CallAnswerRequest request)
        {
            await InvokeHubMethodAsync("SendAnswer", request);
        }

        public async Task SendIceCandidateAsync(IceCandidateRequest request)
        {
            await InvokeHubMethodAsync("SendIceCandidate", request);
        }

        public async Task EndCallAsync(int callSessionId)
        {
            await InvokeHubMethodAsync("EndCall", callSessionId);
        }

        public async Task RejectCallAsync(int callSessionId)
        {
            await InvokeHubMethodAsync("RejectCall", callSessionId);
        }

        public async Task CancelCallAsync(int callSessionId)
        {
            await InvokeHubMethodAsync("CancelCall", callSessionId);
        }

        public async Task ToggleMediaAsync(int callSessionId, bool isAudioEnabled, bool isVideoEnabled)
        {
            await InvokeHubMethodAsync("ToggleMedia", callSessionId, isAudioEnabled, isVideoEnabled);
        }

        public async Task<ActiveCallInfoResponse?> GetActiveCallAsync()
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
                return null;

            try
            {
                return await _hubConnection.InvokeAsync<ActiveCallInfoResponse?>("GetActiveCall");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CallHub] GetActiveCall failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Central hub invocation with proper error handling.
        /// Logs and raises OnCallError instead of swallowing exceptions.
        /// </summary>
        private async Task InvokeHubMethodAsync(string methodName, params object[] args)
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
            {
                OnCallError?.Invoke("Not connected to call server.");
                return;
            }

            try
            {
                await _hubConnection.InvokeCoreAsync(methodName, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CallHub] {methodName} failed: {ex.Message}");
                OnCallError?.Invoke($"Call operation failed: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                }
                finally
                {
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                }
            }
        }
    }
}
