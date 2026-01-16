using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TaskLinkedRunStep
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int RunStepId { get; set; }
        public DateTime LinkedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        // Navigation properties
        public virtual KanbanTask Task { get; set; } = null!;
        public virtual TestRunStep RunStep { get; set; } = null!;
    }
}
