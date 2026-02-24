using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatMessageResponse
    {
        public int Id { get; set; }
        [JsonPropertyName("roomId")]
        public int RoomId { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("userAvatarUrl")]
        public string? UserAvatarUrl { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int? ParentMessageId { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("attachments")]
        public List<ChatMessageAttachmentResponse> Attachments { get; set; } = new();

        [JsonPropertyName("reactions")]
        public List<ChatMessageReactionResponse> Reactions { get; set; } = new();

        [JsonPropertyName("readBy")]
        public List<ChatMessageReadResponse> ReadBy { get; set; } = new();

        [JsonPropertyName("parentMessage")]
        public ChatMessageResponse? ParentMessage { get; set; }

        [JsonPropertyName("isPinned")]
        public bool IsPinned { get; set; }

        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }
    }
}
