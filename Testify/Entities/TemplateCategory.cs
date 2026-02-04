namespace Testify.Entities
{
    public class TemplateCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<TestSuiteTemplate> TestSuiteTemplates { get; set; } = new List<TestSuiteTemplate>();
    }
}
