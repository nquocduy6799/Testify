using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class ChatRoom : AuditEntity
    {
        public int Id { get; set; }
        public string? RoomName { get; set; }
        public ChatRoomType RoomType { get; set; } = ChatRoomType.Private;
        public int? ProjectId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime LastActivityAt { get; set; } = DateTime.Now;
        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public virtual Project? Project { get; set; }
        public new virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ICollection<ChatRoomParticipant> Participants { get; set; } = new List<ChatRoomParticipant>();
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ChatPinnedMessage> PinnedMessages { get; set; } = new List<ChatPinnedMessage>();
    }
}
