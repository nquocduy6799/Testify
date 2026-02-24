using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.DTOs.TaskActivity
{
    public class TaskActivityResponse
    {
        public int Id { get; set; }
        public int KanbanTaskId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
