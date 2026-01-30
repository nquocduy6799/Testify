using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testify.Entities
{
    public class TaskAttachment 
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }
        public string? FilePublicId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; } = string.Empty;
        public virtual KanbanTask? KanbanTask { get; set; }
    }
}
