using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.Meetings;

namespace Testify.Hubs
{
    [Authorize]
    public class MeetingHub : Hub
    {
        private readonly IMeetingRepository _meetingRepo;
        private readonly ILogger<MeetingHub> _logger;

        // Track which users are in which meeting rooms
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, string>> MeetingConnections = new();

        public MeetingHub(IMeetingRepository meetingRepo, ILogger<MeetingHub> logger)
        {
            _meetingRepo = meetingRepo;
            _logger = logger;
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
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Remove from all meeting rooms
                foreach (var kvp in MeetingConnections)
                {
                    if (kvp.Value.TryRemove(userId, out _))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"meeting_{kvp.Key}");
                        await Clients.Group($"meeting_{kvp.Key}")
                            .SendAsync("ParticipantLeft", userId, GetCurrentUserName());

                        _logger.LogInformation("User {UserId} disconnected from meeting {MeetingId}", userId, kvp.Key);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinMeeting(int meetingId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"meeting_{meetingId}");

            // Track connection
            MeetingConnections.AddOrUpdate(
                meetingId,
                _ => { var d = new ConcurrentDictionary<string, string>(); d.TryAdd(userId, Context.ConnectionId); return d; },
                (_, existing) => { existing[userId] = Context.ConnectionId; return existing; }
            );

            // Mark as joined in DB
            await _meetingRepo.JoinMeetingAsync(meetingId, userId);

            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("ParticipantJoined", userId, userName);

            _logger.LogInformation("User {UserId} joined meeting {MeetingId}", userId, meetingId);
        }

        public async Task LeaveMeeting(int meetingId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"meeting_{meetingId}");

            if (MeetingConnections.TryGetValue(meetingId, out var connections))
            {
                connections.TryRemove(userId, out _);
            }

            await _meetingRepo.LeaveMeetingAsync(meetingId, userId);

            await Clients.Group($"meeting_{meetingId}")
                .SendAsync("ParticipantLeft", userId, userName);

            _logger.LogInformation("User {UserId} left meeting {MeetingId}", userId, meetingId);
        }

        /// <summary>
        /// Broadcast a live caption to all other participants in the meeting.
        /// This is for real-time display only — NOT saved to DB.
        /// </summary>
        public async Task SendLiveCaption(int meetingId, string text, bool isFinal)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("LiveCaptionReceived", userId, userName, text.Trim(), isFinal);
        }

        // ===================== WebRTC Signaling =====================

        /// <summary>
        /// Request all existing peers in this meeting to initiate connections to the new joiner.
        /// Called by the new participant after joining.
        /// </summary>
        public async Task RequestPeerConnections(int meetingId)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            // Notify existing participants so they each create an offer to this new user
            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("PeerJoinedForWebRTC", userId, userName);
        }

        /// <summary>
        /// Relay an SDP offer to a specific user in the meeting.
        /// </summary>
        public async Task SendWebRTCOffer(int meetingId, string targetUserId, string sdpOffer)
        {
            var fromUserId = GetCurrentUserId();
            var fromUserName = GetCurrentUserName();

            if (MeetingConnections.TryGetValue(meetingId, out var connections)
                && connections.TryGetValue(targetUserId, out var targetConnId))
            {
                await Clients.Client(targetConnId)
                    .SendAsync("ReceiveWebRTCOffer", fromUserId, fromUserName, sdpOffer);
            }
        }

        /// <summary>
        /// Relay an SDP answer to a specific user in the meeting.
        /// </summary>
        public async Task SendWebRTCAnswer(int meetingId, string targetUserId, string sdpAnswer)
        {
            var fromUserId = GetCurrentUserId();

            if (MeetingConnections.TryGetValue(meetingId, out var connections)
                && connections.TryGetValue(targetUserId, out var targetConnId))
            {
                await Clients.Client(targetConnId)
                    .SendAsync("ReceiveWebRTCAnswer", fromUserId, sdpAnswer);
            }
        }

        /// <summary>
        /// Relay an ICE candidate to a specific user in the meeting.
        /// </summary>
        public async Task SendIceCandidate(int meetingId, string targetUserId, string candidate, string? sdpMid, int sdpMLineIndex)
        {
            var fromUserId = GetCurrentUserId();

            if (MeetingConnections.TryGetValue(meetingId, out var connections)
                && connections.TryGetValue(targetUserId, out var targetConnId))
            {
                await Clients.Client(targetConnId)
                    .SendAsync("ReceiveIceCandidate", fromUserId, candidate, sdpMid, sdpMLineIndex);
            }
        }

        /// <summary>
        /// Notify peers that this user toggled their camera/mic state.
        /// </summary>
        public async Task NotifyMediaStateChanged(int meetingId, bool isCameraOn, bool isMicOn)
        {
            var userId = GetCurrentUserId();

            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("PeerMediaStateChanged", userId, isCameraOn, isMicOn);
        }

        /// <summary>
        /// Notify peers that this user raised/lowered their hand.
        /// </summary>
        public async Task NotifyHandRaised(int meetingId, bool isRaised)
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("PeerHandRaised", userId, userName, isRaised);
        }

        /// <summary>
        /// Send a chat message to all participants in the meeting.
        /// </summary>
        public async Task SendChatMessage(int meetingId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            var message = new MeetingChatMessage
            {
                UserId = userId,
                UserName = userName,
                Content = content.Trim(),
                Timestamp = DateTime.UtcNow
            };

            // Also save to transcript so AI summary includes chat messages
            await _meetingRepo.AddTranscriptAsync(meetingId, userId, $"[Chat] {content.Trim()}");

            await Clients.OthersInGroup($"meeting_{meetingId}")
                .SendAsync("ChatMessageReceived", message);
        }
    }
}
