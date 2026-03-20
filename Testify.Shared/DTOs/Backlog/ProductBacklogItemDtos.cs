using System;
using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Backlog
{
    public class CreateProductBacklogItemRequest
    {
        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? AcceptanceCriteria { get; set; }

        public BacklogItemType Type { get; set; } = BacklogItemType.UserStory;

        public BacklogItemStatus Status { get; set; } = BacklogItemStatus.New;

        public BacklogItemPriority Priority { get; set; } = BacklogItemPriority.Medium;

        [Range(0, int.MaxValue, ErrorMessage = "Story points must be a non-negative number")]
        public int StoryPoints { get; set; } = 0;

        public int Order { get; set; } = 0;
    }

    public class UpdateProductBacklogItemRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 255 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? AcceptanceCriteria { get; set; }

        public BacklogItemType Type { get; set; } = BacklogItemType.UserStory;

        public BacklogItemStatus Status { get; set; } = BacklogItemStatus.New;

        public BacklogItemPriority Priority { get; set; } = BacklogItemPriority.Medium;

        [Range(0, int.MaxValue, ErrorMessage = "Story points must be a non-negative number")]
        public int StoryPoints { get; set; } = 0;

        public int Order { get; set; } = 0;
    }

    public class ProductBacklogItemResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public BacklogItemType Type { get; set; }
        public BacklogItemStatus Status { get; set; }
        public BacklogItemPriority Priority { get; set; }
        public int StoryPoints { get; set; }
        public int Order { get; set; }
        public int PrintBacklogItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }
}
