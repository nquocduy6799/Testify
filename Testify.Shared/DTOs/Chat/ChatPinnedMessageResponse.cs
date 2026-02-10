using System;
using System.Text.Json.Serialization;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatPinnedMessageResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("roomId")]
        public int RoomId { get; set; }

        [JsonPropertyName("messageId")]
        public int MessageId { get; set; }

        [JsonPropertyName("pinnedByUserId")]
        public string PinnedByUserId { get; set; } = string.Empty;

        [JsonPropertyName("pinnedByUserName")]
        public string PinnedByUserName { get; set; } = string.Empty;

        [JsonPropertyName("pinnedAt")]
        public DateTime PinnedAt { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        /// <summary>
        /// The pinned message content snapshot
        /// </summary>
        [JsonPropertyName("message")]
        public ChatMessageResponse Message { get; set; } = null!;
    }
}
