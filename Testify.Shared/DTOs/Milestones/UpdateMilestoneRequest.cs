using System;
using System.ComponentModel.DataAnnotations;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Shared.DTOs.Milestones
{
    public class UpdateMilestoneRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public MilestoneStatus Status { get; set; } = MilestoneStatus.Active;

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
