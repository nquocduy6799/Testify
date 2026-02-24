using System.Text.Json.Serialization;

namespace Testify.Shared.DTOs.Chat
{
    public class ChatCurrentUserResponse
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
    }
}
