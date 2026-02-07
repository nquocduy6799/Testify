using Testify.Shared.Enums;

namespace Testify.Client.Models
{
    public class CurrentUserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
