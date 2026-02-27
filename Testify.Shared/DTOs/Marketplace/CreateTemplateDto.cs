namespace Testify.Shared.DTOs.Marketplace
{
    public class CreateTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public bool IsPublic { get; set; } = true;
        public List<string> Tags { get; set; } = new();
    }
}
