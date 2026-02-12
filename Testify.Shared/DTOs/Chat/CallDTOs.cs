using System;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    /// <summary>
    /// Sent by caller to initiate a call via SignalR
    /// </summary>
    public class CallOfferRequest
    {
        public int RoomId { get; set; }
        public CallType CallType { get; set; }
        public string SdpOffer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sent by callee to accept a call via SignalR
    /// </summary>
    public class CallAnswerRequest
    {
        public int CallSessionId { get; set; }
        public string SdpAnswer { get; set; } = string.Empty;
    }

    /// <summary>
    /// ICE candidate exchange between peers via SignalR
    /// </summary>
    public class IceCandidateRequest
    {
        public int CallSessionId { get; set; }
        public string Candidate { get; set; } = string.Empty;
        public string? SdpMid { get; set; }
        public int SdpMLineIndex { get; set; }
    }

    /// <summary>
    /// Incoming call notification sent to callee
    /// </summary>
    public class IncomingCallResponse
    {
        public int CallSessionId { get; set; }
        public int RoomId { get; set; }
        public CallType CallType { get; set; }
        public string CallerUserId { get; set; } = string.Empty;
        public string CallerName { get; set; } = string.Empty;
        public string? CallerAvatarUrl { get; set; }
        public string SdpOffer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Call answered notification sent to caller
    /// </summary>
    public class CallAnsweredResponse
    {
        public int CallSessionId { get; set; }
        public string SdpAnswer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Call session info for call history
    /// </summary>
    public class CallSessionResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string CallerUserId { get; set; } = string.Empty;
        public string CallerName { get; set; } = string.Empty;
        public string CalleeUserId { get; set; } = string.Empty;
        public string CalleeName { get; set; } = string.Empty;
        public CallType CallType { get; set; }
        public CallStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
    }

    /// <summary>
    /// Call ended notification
    /// </summary>
    public class CallEndedResponse
    {
        public int CallSessionId { get; set; }
        public CallStatus Reason { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
