namespace Testify.Shared.DTOs.Tags
{
    public class TagResponse
    {
        public int Id { get; set; }
        public string TagName { get; set; } = string.Empty;
    }

    public class CreateTagRequest
    {
        public string TagName { get; set; } = string.Empty;
    }
}
