namespace Testify.Shared.DTOs.TestTemplates
{
    public class BulkMoveRequest
    {
        public List<int> TemplateIds { get; set; } = new();
        public int? TargetFolderId { get; set; }
    }
}
