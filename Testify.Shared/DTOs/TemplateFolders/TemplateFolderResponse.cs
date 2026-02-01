using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.DTOs.TemplateFolders
{
    public class TemplateFolderResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<TemplateFolderResponse> SubFolders { get; set; } = new();
    }
}
