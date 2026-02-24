using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.ComponentModel.DataAnnotations.Schema;
using Testify.Data;

namespace Testify.Entities
{
    public class TemplateFolder
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual TemplateFolder? Parent { get; set; }
        public virtual ICollection<TemplateFolder> SubFolders { get; set; } = new List<TemplateFolder>();
    }
}

