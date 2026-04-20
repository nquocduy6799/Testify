using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Enums
{
    public enum ChatRoomType
    {
        Private,
        Group,
        Project
    }

    public enum ChatParticipantRole
    {
        Member,
        Admin,
        Owner
    }

    public enum MessageType
    {
        Text,
        Image,
        File,
        System,
        Call
    }

    public enum NotificationType
    {
        // Message Events
        NewMessage = 1,
        Mention = 2,
        Reply = 3,
        Reaction = 4,

        // Room Events
        RoomInvite = 10,
        RoomRemove = 11,
        RoomUpdate = 12,

        // System/Media
        FileShared = 20,
        MissedCall = 21,

        // Project Events
        ProjectInvitation = 30,

        // Meeting Events
        MeetingCreated = 39,
        MeetingStarted = 40,
        MeetingSummaryReady = 41
    }

    public enum CallType
    {
        Audio,
        Video
    }

    public enum CallStatus
    {
        Ringing,
        Active,
        Ended,
        Missed,
        Rejected,
        Cancelled
    }
}
