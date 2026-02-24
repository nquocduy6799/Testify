namespace Testify.Entities
{
    public class TemplateTag
    {
        public int Id { get; set; }
        public string? TagName { get; set; }

        // Navigation property
        public virtual ICollection<TestSuiteTemplateTag> TestSuiteTemplateTags { get; set; } = new List<TestSuiteTemplateTag>();
    }
}
