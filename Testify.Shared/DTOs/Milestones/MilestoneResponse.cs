using System;
using Testify.Shared.DTOs.KanbanTasks;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Shared.DTOs.Milestones
{
    public class MilestoneResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MilestoneStatus Status { get; set; } = MilestoneStatus.Active;
        public List<KanbanTaskResponse> Tasks { get; set; } = new();

        public bool IsValidDateRange()
        {
            return EndDate >= StartDate;
        }
    }
}
