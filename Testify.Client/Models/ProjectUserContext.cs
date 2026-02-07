using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Client.Models
{
    public class ProjectUserContext : CurrentUserInfo
    {
        public ProjectRole? ProjectRole { get; set; }
        public bool IsPM { get; set; }
        public List<TeamMemberResponse> TeamMembers { get; set; } = new();
    }
}
