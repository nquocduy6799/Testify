using System;
using System.Collections.Generic;
using System.Text;
using Testify.Shared.DTOs.TaskAttachments;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.KanbanTasks
{
    public class KanbanTaskResponse
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public int? TaskPlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public KanbanTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        public string? AssigneeId { get; set; }

        // Flattened property: Useful for displaying the user's name without fetching the full object
        public string? AssigneeName { get; set; }
        public string? AssigneeAvatarUrl { get; set; }
        public string? AssigneeRole { get; set; }

        public TaskType Type { get; set; }

        // Audit fields (from AuditEntity) are often useful for display
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int TestPlanCount { get; set; }
        public List<TaskAttachmentResponse> Attachments { get; set; } = new();
    }
}
