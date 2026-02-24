using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.TestPlans
{
    public class TestPlanResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public int? MilestoneId { get; set; }
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
        public TestPlanOutcome? Outcome { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
        public List<TestSuiteResponse> TestSuites { get; set; } = new List<TestSuiteResponse>();
    }

    public class CreateTestPlanRequest
    {
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public int? MilestoneId { get; set; }
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
        public List<TestSuiteResponse> TestSuites { get; set; } = new List<TestSuiteResponse>();
    }

    public class UpdateTestPlanRequest
    {
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public int? MilestoneId { get; set; }
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
        public List<TestSuiteResponse> TestSuites { get; set; } = new List<TestSuiteResponse>();
    }
}




//public class TestPlan : AuditEntity
//{
//    public int Id { get; set; }
//    public int ProjectId { get; set; }
//    public int? TaskId { get; set; }
//    public int? MilestoneId { get; set; }
//    public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
//    public string Name { get; set; } = string.Empty;
//    public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
//    public TestPlanOutcome? Outcome { get; set; }
//    public DateTime StartedAt { get; set; }
//    public DateTime? CompletedAt { get; set; }
//    public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;

//    // Navigation properties
//    public virtual Project Project { get; set; } = null!;
//    public virtual KanbanTask? Task { get; set; }
//    public virtual Milestone? Milestone { get; set; }
//    public virtual ICollection<TestPlanSuite> TestPlanSuites { get; set; } = new List<TestPlanSuite>();
//    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
//}