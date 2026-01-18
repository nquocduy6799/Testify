using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class Milestone : AuditEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Planned, Closed

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<KanbanTask> Tasks { get; set; } = new List<KanbanTask>();
        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
    }
}
