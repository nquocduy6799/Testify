using System;
using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Milestones
{
    public class CreateMilestoneDTO
    {
        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        
        [Required]
        public int ProjectId { get; set; }

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
