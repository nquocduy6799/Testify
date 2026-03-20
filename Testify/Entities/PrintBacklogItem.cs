using Testify.Data;
using Testify.Shared.Enums;

namespace Testify.Entities
{
    public class PrintBacklogItem : AuditEntity
    {
        public int Id { get; set; }

        public int PrintBacklogId { get; set; }

        public int? ProductBacklogItemId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public BacklogItemStatus Status { get; set; } = BacklogItemStatus.New;

        public BacklogItemPriority Priority { get; set; } = BacklogItemPriority.Medium;

        public int StoryPoints { get; set; } = 0;

        public int Order { get; set; } = 0;

        public string? AssigneeId { get; set; }

        // Navigation properties
        public virtual PrintBacklog PrintBacklog { get; set; } = null!;
        public virtual ProductBacklogItem? ProductBacklogItem { get; set; }
        public virtual ApplicationUser? Assignee { get; set; }
    }
}
