using System;

namespace Testify.Shared.DTOs.Milestones
{
    public class MilestoneDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Planned";
        public List<Testify.Shared.DTOs.Tasks.TaskDto> Tasks { get; set; } = new();

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
