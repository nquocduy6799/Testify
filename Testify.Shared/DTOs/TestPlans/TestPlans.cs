using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public string Name { get; set; } = string.Empty;
        public TestPlanStatus Status { get; set; } = TestPlanStatus.Draft;
        public TestPlanOutcome? Outcome { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
    }

    public class CreateTestPlanRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public int? TaskId { get; set; }
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
    }

    public class UpdateTestPlanRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public int? TaskId { get; set; }
        public TestPlanScope Scope { get; set; } = TestPlanScope.Project;
        public TestPlanPriority Priority { get; set; } = TestPlanPriority.Medium;
    }
}
