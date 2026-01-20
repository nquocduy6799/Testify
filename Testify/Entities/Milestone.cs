using System;
using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Entities
{
    public class Milestone : AuditEntity
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        public MilestoneStatus Status { get; set; } = MilestoneStatus.Active;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<KanbanTask> KanbanTasks { get; set; } = new List<KanbanTask>();
        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}



//namespace Testify.Entities
//{
//    public class Milestone : AuditEntity
//    {
//        public int Id { get; set; }
//        public int ProjectId { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public DateTime StartDate { get; set; }
//        public DateTime EndDate { get; set; }
//        public string Status { get; set; } = "Active"; // Active, Completed, Planned, Closed

//        // Navigation properties
//        public virtual Project Project { get; set; } = null!;
//        public virtual ICollection<KanbanTask> Tasks { get; set; } = new List<KanbanTask>();
//        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
//    }
//}