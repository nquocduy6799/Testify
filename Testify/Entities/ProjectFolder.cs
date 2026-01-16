using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class ProjectFolder
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ProjectFolder? Parent { get; set; }
        public virtual ICollection<ProjectFolder> SubFolders { get; set; } = new List<ProjectFolder>();
        public virtual ICollection<TestSuite> TestSuites { get; set; } = new List<TestSuite>();
    }
}
