using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;

namespace Testify.Entities
{
    public class Project : AuditEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Client { get; set; }
        public int Progress { get; set; } = 0;
        public DateTime? Deadline { get; set; }
        public int MembersCount { get; set; } = 0;
        public int BugThreshold { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<ProjectTeamMember> TeamMembers { get; set; } =
            new List<ProjectTeamMember>();
        public virtual ICollection<ProjectFolder> Folders { get; set; } = new List<ProjectFolder>();

        public virtual ICollection<TestSuite> TestSuites { get; set; } = new List<TestSuite>();
        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
        public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
    }
}
