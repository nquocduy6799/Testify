using System;
using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Milestones
{
    public class UpdateMilestoneDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "Planned";

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
