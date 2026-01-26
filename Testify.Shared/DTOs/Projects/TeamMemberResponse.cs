using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Projects
{
    public class TeamMemberResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
