using System.Collections.Generic;

namespace Testify.Shared.DTOs.Chat
{
    public class SearchMessagesResponse
    {
        public List<ChatMessageResponse> Messages { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
