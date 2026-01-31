using System.ComponentModel.DataAnnotations;
using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class TaskActivity
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual KanbanTask KanbanTask { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
