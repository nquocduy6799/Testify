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
        System
    }

    public enum NotificationType
    {
        Info = 0,
        Warning = 1,
        Success = 2,
        Error = 3
    }
}
