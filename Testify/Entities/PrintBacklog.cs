using System.ComponentModel.DataAnnotations;

namespace Testify.Entities
{
    public class PrintBacklog : AuditEntity
    {
        public int Id { get; set; }

        public int MilestoneId { get; set; }

        [Required(ErrorMessage = "PrintBacklog name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation properties
        public virtual Milestone Milestone { get; set; } = null!;
        public virtual ICollection<PrintBacklogItem> Items { get; set; } = new List<PrintBacklogItem>();
    }
}
