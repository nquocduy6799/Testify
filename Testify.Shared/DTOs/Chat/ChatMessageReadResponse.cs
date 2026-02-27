using System;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatMessageReadResponse
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ReadAt { get; set; }
    }
}
