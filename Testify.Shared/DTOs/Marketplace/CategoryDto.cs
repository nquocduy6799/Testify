namespace Testify.Shared.DTOs.Marketplace
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Subcategories { get; set; } = new();
    }
}
