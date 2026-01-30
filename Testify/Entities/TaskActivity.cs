using System.ComponentModel.DataAnnotations;
using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class TaskActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OldValue { get; set; }

        [MaxLength(255)]
        public string? NewValue { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual KanbanTask KanbanTask { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
