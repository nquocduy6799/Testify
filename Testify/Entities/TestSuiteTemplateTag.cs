namespace Testify.Entities
{
    public class TestSuiteTemplateTag
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public virtual TestSuiteTemplate Template { get; set; } = null!;
        public virtual TemplateTag Tag { get; set; } = null!;
    }
}
