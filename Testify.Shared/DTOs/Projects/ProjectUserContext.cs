using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Projects
{
    public class ProjectUserContext
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public ProjectRole? ProjectRole { get; set; }
        public bool IsPM { get; set; }
        public List<TeamMemberResponse> TeamMembers { get; set; } = new();

    }
}
