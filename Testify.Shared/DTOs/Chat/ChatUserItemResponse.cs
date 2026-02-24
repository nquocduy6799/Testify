namespace Testify.Shared.DTOs.Chat
{
    public class ChatUserItemResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; }
    }
}
