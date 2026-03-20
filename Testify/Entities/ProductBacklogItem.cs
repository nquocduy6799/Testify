using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class ProductBacklogItem : AuditEntity
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? AcceptanceCriteria { get; set; }

        public BacklogItemType Type { get; set; } = BacklogItemType.UserStory;

        public BacklogItemStatus Status { get; set; } = BacklogItemStatus.New;

        public BacklogItemPriority Priority { get; set; } = BacklogItemPriority.Medium;

        public int StoryPoints { get; set; } = 0;

        public int Order { get; set; } = 0;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<PrintBacklogItem> PrintBacklogItems { get; set; } = new List<PrintBacklogItem>();
    }
}
