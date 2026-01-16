using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;

namespace Testify.Entities
{
    public class ChatMessageReaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Reaction { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ChatMessage Message { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
