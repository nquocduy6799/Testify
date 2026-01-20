using System;
using System.Collections.Generic;
using System.Text;
using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class KanbanTask : AuditEntity
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public KanbanTaskStatus Status { get; set; } = KanbanTaskStatus.ToDo;
        public int Priority { get; set; } = 1;
        public string? AssigneeId { get; set; }
        public TaskType Type { get; set; } = TaskType.Feature;

        // Navigation properties

        public virtual ApplicationUser? Assignee { get; set; }
        public virtual ICollection<TestPlan> TestPlans { get; set; } = new List<TestPlan>();
        public virtual ICollection<TaskLinkedRunStep> LinkedRunSteps { get; set; } =
            new List<TaskLinkedRunStep>();
    }
}
