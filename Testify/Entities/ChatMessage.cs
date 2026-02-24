using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class ChatMessage : AuditEntity
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string Content { get; set; } = string.Empty;
        public new bool IsDeleted { get; set; } = false;
        public int? ParentMessageId { get; set; }
        public string? Metadata { get; set; }

        // Navigation properties
        public virtual ChatRoom Room { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ChatMessage? ParentMessage { get; set; }
        public virtual ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ChatMessageAttachment> Attachments { get; set; } = new List<ChatMessageAttachment>();
        public virtual ICollection<ChatMessageReaction> Reactions { get; set; } = new List<ChatMessageReaction>();
        public virtual ICollection<ChatMessageRead> Reads { get; set; } = new List<ChatMessageRead>();
        public virtual ICollection<ChatPinnedMessage> PinnedInRooms { get; set; } = new List<ChatPinnedMessage>();
    }
}
