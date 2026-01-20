namespace Testify.Shared.DTOs.Tasks
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = "Todo"; // Todo, InProgress, Done, Cancelled
    }
}
