using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.DTOs.Projects
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Client { get; set; }
        public int Progress { get; set; }
        public DateTime? Deadline { get; set; }
        public int MembersCount { get; set; }
        public int BugThreshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
