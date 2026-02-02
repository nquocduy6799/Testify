using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Testify.Shared.DTOs.TemplateFolders
{
    public class CreateTemplateFolderRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
    }
}