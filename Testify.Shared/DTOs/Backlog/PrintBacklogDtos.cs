using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Backlog
{
    public class CreatePrintBacklogRequest
    {
        [Required(ErrorMessage = "Milestone is required")]
        public int MilestoneId { get; set; }

        [Required(ErrorMessage = "PrintBacklog name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdatePrintBacklogRequest
    {
        [Required(ErrorMessage = "PrintBacklog name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class PrintBacklogResponse
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ItemCount { get; set; }
        public List<PrintBacklogItemResponse> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }
}
