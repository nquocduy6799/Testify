using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.KanbanTasks
{
    public class CreateKanbanTaskRequest
    {
        [Required(ErrorMessage = "Milestone is required.")]
        public int MilestoneId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters.")]
        public string Title { get; set; } = string.Empty;

        // You can typically omit Status on creation (defaulting to ToDo in logic),
        // but keeping it allows creating a task directly in "In Progress" if needed.
        public KanbanTaskStatus Status { get; set; } = KanbanTaskStatus.ToDo;

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public int Priority { get; set; } = 1;

        public string? AssigneeId { get; set; }

        public TaskType Type { get; set; } = TaskType.Feature;

    }
}
