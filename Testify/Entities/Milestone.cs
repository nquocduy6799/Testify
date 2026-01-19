using System;
using System.ComponentModel.DataAnnotations;

namespace Testify.Entities
{
    public class Milestone : AuditEntity
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        public string Status { get; set; } = "Planned"; // Planned, Active, Completed, OnHold

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
