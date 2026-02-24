using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using Testify.Shared.Settings;

namespace Testify.Client.Features.Chat.Services
{
    public class WebRtcService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IConfiguration _configuration;
        private DotNetObjectReference<WebRtcService>? _dotNetRef;

        // Events for signaling back to Blazor
        public event Action<string, string?, int>? IceCandidateGenerated; // candidate, sdpMid, sdpMLineIndex
        public event Action<string>? ConnectionStateChanged;
        public event Action<string>? IceConnectionStateChanged;
        public event Action<string>? MediaError;

        public WebRtcService(IJSRuntime jsRuntime, IConfiguration configuration)
        {
            _jsRuntime = jsRuntime;
            _configuration = configuration;
        }

        /// <summary>
        /// Initialize WebRTC peer connection and get local media
        /// </summary>
        public async Task<bool> InitializeAsync(bool isVideo)
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            // Read ICE servers from configuration
            var iceServers = _configuration.GetSection("WebRtc:IceServers")
                .Get<List<IceServerConfig>>()?
                .Select(s => new { urls = s.Urls, username = s.Username, credential = s.Credential })
                .ToArray();

            return await _jsRuntime.InvokeAsync<bool>("WebRtcInterop.initialize", _dotNetRef, isVideo, iceServers);
        }

        /// <summary>
        /// Create SDP offer (caller side)
        /// </summary>
        public async Task<string?> CreateOfferAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("WebRtcInterop.createOffer");
        }

        /// <summary>
        /// Create SDP answer (callee side)
        /// </summary>
        public async Task<string?> CreateAnswerAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("WebRtcInterop.createAnswer");
        }

        /// <summary>
        /// Set remote SDP description
        /// </summary>
        public async Task<bool> SetRemoteDescriptionAsync(string sdpJson)
        {
            return await _jsRuntime.InvokeAsync<bool>("WebRtcInterop.setRemoteDescription", sdpJson);
        }

        /// <summary>
        /// Add a received ICE candidate
        /// </summary>
        public async Task<bool> AddIceCandidateAsync(string candidate, string? sdpMid, int sdpMLineIndex)
        {
            return await _jsRuntime.InvokeAsync<bool>("WebRtcInterop.addIceCandidate", candidate, sdpMid, sdpMLineIndex);
        }

        /// <summary>
        /// Toggle audio track
        /// </summary>
        public async Task ToggleAudioAsync(bool enabled)
        {
            await _jsRuntime.InvokeVoidAsync("WebRtcInterop.toggleAudio", enabled);
        }

        /// <summary>
        /// Toggle video track
        /// </summary>
        public async Task ToggleVideoAsync(bool enabled)
        {
            await _jsRuntime.InvokeVoidAsync("WebRtcInterop.toggleVideo", enabled);
        }

        /// <summary>
        /// Check if local stream has video track
        /// </summary>
        public async Task<bool> HasVideoTrackAsync()
        {
            return await _jsRuntime.InvokeAsync<bool>("WebRtcInterop.hasVideoTrack");
        }

        /// <summary>
        /// Play ringing tone for incoming call
        /// </summary>
        public async Task PlayRingtoneAsync()
        {
            await _jsRuntime.InvokeVoidAsync("WebRtcInterop.playRingtone");
        }

        /// <summary>
        /// Stop ringing tone
        /// </summary>
        public async Task StopRingtoneAsync()
        {
            await _jsRuntime.InvokeVoidAsync("WebRtcInterop.stopRingtone");
        }

        /// <summary>
        /// Cleanup all WebRTC resources
        /// </summary>
        public async Task DisposeWebRtcAsync()
        {
            await _jsRuntime.InvokeVoidAsync("WebRtcInterop.dispose");
        }

        #region JS Interop Callbacks

        [JSInvokable]
        public void OnIceCandidateGenerated(string candidate, string? sdpMid, int sdpMLineIndex)
        {
            IceCandidateGenerated?.Invoke(candidate, sdpMid, sdpMLineIndex);
        }

        [JSInvokable]
        public void OnConnectionStateChanged(string state)
        {
            ConnectionStateChanged?.Invoke(state);
        }

        [JSInvokable]
        public void OnIceConnectionStateChanged(string state)
        {
            IceConnectionStateChanged?.Invoke(state);
        }

        [JSInvokable]
        public void OnMediaError(string error)
        {
            MediaError?.Invoke(error);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            await DisposeWebRtcAsync();
            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }
    }
}
