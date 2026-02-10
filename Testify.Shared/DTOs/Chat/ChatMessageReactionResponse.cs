using System;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatMessageReactionResponse
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
