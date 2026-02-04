namespace Testify.Entities
{
    public class TemplateCategory
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
        public virtual TemplateCategory? ParentCategory { get; set; }

        public virtual ICollection<TemplateCategory> SubCategories { get; set; } = new List<TemplateCategory>();

        public virtual ICollection<TestSuiteTemplate> TestSuiteTemplates { get; set; }= new List<TestSuiteTemplate>();
    }
}