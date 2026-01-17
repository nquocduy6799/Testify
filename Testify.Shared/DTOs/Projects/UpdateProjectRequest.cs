using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Testify.Shared.DTOs.Projects
{
    public class UpdateProjectRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }

        [StringLength(200)]
        public string? Client { get; set; }

        [Range(0, 100)]
        public int Progress { get; set; }

        public DateTime? Deadline { get; set; }

        [Range(0, 100)]
        public int BugThreshold { get; set; }
    }
}
