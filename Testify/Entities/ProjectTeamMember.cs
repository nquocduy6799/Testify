using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class ProjectTeamMember
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ProjectRole Role { get; set; } = ProjectRole.PM;
        public DateTime JoinedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
