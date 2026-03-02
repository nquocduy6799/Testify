using System;
using System.ComponentModel.DataAnnotations;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Shared.DTOs.Milestones
{
    public class CreateMilestoneRequest
    {
        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        
        [Required]
        public int ProjectId { get; set; }

        public MilestoneStatus Status { get; set; } = MilestoneStatus.Active;

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }

        /// <summary>
        /// Only Active and Planned are valid statuses when creating a new milestone.
        /// </summary>
        public bool IsValidStatusForCreation()
        {
            return Status == MilestoneStatus.Active || Status == MilestoneStatus.Planned;
        }
    }
}
