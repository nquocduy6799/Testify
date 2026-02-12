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

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public CallHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartAsync()
        {
            if (_hubConnection != null) return;

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

        public async Task SendOfferAsync(CallOfferRequest request)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("SendOffer", request); } catch { }
        }

        public async Task SendAnswerAsync(CallAnswerRequest request)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("SendAnswer", request); } catch { }
        }

        public async Task SendIceCandidateAsync(IceCandidateRequest request)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("SendIceCandidate", request); } catch { }
        }

        public async Task EndCallAsync(int callSessionId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("EndCall", callSessionId); } catch { }
        }

        public async Task RejectCallAsync(int callSessionId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("RejectCall", callSessionId); } catch { }
        }

        public async Task CancelCallAsync(int callSessionId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("CancelCall", callSessionId); } catch { }
        }

        public async Task ToggleMediaAsync(int callSessionId, bool isAudioEnabled, bool isVideoEnabled)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            try { await _hubConnection.InvokeAsync("ToggleMedia", callSessionId, isAudioEnabled, isVideoEnabled); } catch { }
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
