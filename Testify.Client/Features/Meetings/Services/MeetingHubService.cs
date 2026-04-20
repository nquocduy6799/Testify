using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Testify.Shared.DTOs.Meetings;

namespace Testify.Client.Features.Meetings.Services
{
    public class MeetingHubService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        private int? _currentMeetingId;

        // Events
        public event Action<string, string>? OnParticipantJoined; // userId, userName
        public event Action<string, string>? OnParticipantLeft; // userId, userName
        public event Action<MeetingTranscriptEntry>? OnTranscriptReceived;
        public event Action<int>? OnMeetingEnded; // meetingId
        public event Action<MeetingResponse>? OnMeetingStarted;

        // WebRTC signaling events
        public event Action<string, string>? OnPeerJoinedForWebRTC; // peerId, peerName
        public event Action<string, string, string>? OnReceiveWebRTCOffer; // fromUserId, fromUserName, sdpOffer
        public event Action<string, string>? OnReceiveWebRTCAnswer; // fromUserId, sdpAnswer
        public event Action<string, string, string?, int>? OnReceiveIceCandidate; // fromUserId, candidate, sdpMid, sdpMLineIndex
        public event Action<string, bool, bool>? OnPeerMediaStateChanged; // userId, isCameraOn, isMicOn
        public event Action<string, string, bool>? OnPeerHandRaised; // userId, userName, isRaised
        public event Action<MeetingChatMessage>? OnChatMessageReceived;
        public event Action<string, string, string, bool>? OnLiveCaptionReceived; // userId, userName, text, isFinal

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public MeetingHubService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartAsync()
        {
            if (_hubConnection?.State == HubConnectionState.Connected) return;

            if (_hubConnection != null)
            {
                try { await _hubConnection.DisposeAsync(); } catch { }
                _hubConnection = null;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/meeting"))
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                .Build();

            _hubConnection.On<string, string>("ParticipantJoined", (uid, name) => OnParticipantJoined?.Invoke(uid, name));
            _hubConnection.On<string, string>("ParticipantLeft", (uid, name) => OnParticipantLeft?.Invoke(uid, name));
            _hubConnection.On<MeetingTranscriptEntry>("TranscriptReceived", entry => OnTranscriptReceived?.Invoke(entry));
            _hubConnection.On<int>("MeetingEnded", id => OnMeetingEnded?.Invoke(id));
            _hubConnection.On<MeetingResponse>("MeetingStarted", meeting => OnMeetingStarted?.Invoke(meeting));

            // WebRTC signaling handlers
            _hubConnection.On<string, string>("PeerJoinedForWebRTC", (uid, name) => OnPeerJoinedForWebRTC?.Invoke(uid, name));
            _hubConnection.On<string, string, string>("ReceiveWebRTCOffer", (uid, name, sdp) => OnReceiveWebRTCOffer?.Invoke(uid, name, sdp));
            _hubConnection.On<string, string>("ReceiveWebRTCAnswer", (uid, sdp) => OnReceiveWebRTCAnswer?.Invoke(uid, sdp));
            _hubConnection.On<string, string, string?, int>("ReceiveIceCandidate", (uid, cand, mid, idx) => OnReceiveIceCandidate?.Invoke(uid, cand, mid, idx));
            _hubConnection.On<string, bool, bool>("PeerMediaStateChanged", (uid, cam, mic) => OnPeerMediaStateChanged?.Invoke(uid, cam, mic));
            _hubConnection.On<string, string, bool>("PeerHandRaised", (uid, name, raised) => OnPeerHandRaised?.Invoke(uid, name, raised));
            _hubConnection.On<MeetingChatMessage>("ChatMessageReceived", msg => OnChatMessageReceived?.Invoke(msg));
            _hubConnection.On<string, string, string, bool>("LiveCaptionReceived", (uid, name, text, isFinal) => OnLiveCaptionReceived?.Invoke(uid, name, text, isFinal));

            _hubConnection.Reconnected += async _ =>
            {
                if (_currentMeetingId.HasValue)
                    await JoinMeetingAsync(_currentMeetingId.Value);
            };

            await _hubConnection.StartAsync();
        }

        public async Task JoinMeetingAsync(int meetingId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;

            _currentMeetingId = meetingId;
            await _hubConnection.InvokeAsync("JoinMeeting", meetingId);
        }

        public async Task LeaveMeetingAsync(int meetingId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;

            _currentMeetingId = null;
            await _hubConnection.InvokeAsync("LeaveMeeting", meetingId);
        }

        public async Task SendTranscriptAsync(int meetingId, string text)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            if (string.IsNullOrWhiteSpace(text)) return;

            await _hubConnection.InvokeAsync("SendTranscript", meetingId, text);
        }

        // ===================== WebRTC Signaling Methods =====================

        public async Task RequestPeerConnectionsAsync(int meetingId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("RequestPeerConnections", meetingId);
        }

        public async Task SendWebRTCOfferAsync(int meetingId, string targetUserId, string sdpOffer)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendWebRTCOffer", meetingId, targetUserId, sdpOffer);
        }

        public async Task SendWebRTCAnswerAsync(int meetingId, string targetUserId, string sdpAnswer)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendWebRTCAnswer", meetingId, targetUserId, sdpAnswer);
        }

        public async Task SendIceCandidateAsync(int meetingId, string targetUserId, string candidate, string? sdpMid, int sdpMLineIndex)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendIceCandidate", meetingId, targetUserId, candidate, sdpMid, sdpMLineIndex);
        }

        public async Task NotifyMediaStateChangedAsync(int meetingId, bool isCameraOn, bool isMicOn)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("NotifyMediaStateChanged", meetingId, isCameraOn, isMicOn);
        }

        public async Task NotifyHandRaisedAsync(int meetingId, bool isRaised)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("NotifyHandRaised", meetingId, isRaised);
        }

        public async Task SendChatMessageAsync(int meetingId, string content)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            if (string.IsNullOrWhiteSpace(content)) return;
            await _hubConnection.InvokeAsync("SendChatMessage", meetingId, content);
        }

        public async Task SendLiveCaptionAsync(int meetingId, string text, bool isFinal)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            await _hubConnection.InvokeAsync("SendLiveCaption", meetingId, text, isFinal);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    if (_currentMeetingId.HasValue)
                        await LeaveMeetingAsync(_currentMeetingId.Value);
                    await _hubConnection.DisposeAsync();
                }
                catch { }
            }
        }
    }
}
