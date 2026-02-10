using Testify.Entities;
using Testify.Shared.Enums;

public class TestPlan : AuditEntity
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int? MilestoneId { get; set; }  
    public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
    public string Name { get; set; } = string.Empty;
    public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
    public TestPlanOutcome? Outcome { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual KanbanTask? Task { get; set; }
    public virtual Milestone? Milestone { get; set; }  
    public virtual ICollection<TestPlanSuite> TestPlanSuites { get; set; } = new List<TestPlanSuite>();
    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}