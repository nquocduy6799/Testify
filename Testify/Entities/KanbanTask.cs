using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Entities;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class KanbanTask : AuditEntity
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public KanbanTaskStatus Status { get; set; } = KanbanTaskStatus.ToDo;
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public string? AssigneeId { get; set; }
        public string? DevelopedById { get; set; }
        public string? TestedById { get; set; }
        public TaskType Type { get; set; } = TaskType.Feature;

        // Navigation properties
        public virtual Milestone Milestone { get; set; } = null!;
        public virtual ApplicationUser? Assignee { get; set; }
        public virtual ApplicationUser? DevelopedBy { get; set; }
        public virtual ApplicationUser? TestedBy { get; set; }
        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
        public virtual ICollection<TaskLinkedRunStep> LinkedRunSteps { get; set; } = new List<TaskLinkedRunStep>();
        public virtual ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    }
}



