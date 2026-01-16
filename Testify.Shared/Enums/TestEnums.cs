using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Enums
{
    public enum TestCasePriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum TestPlanScope
    {
        Project,
        Milestone,
        Task
    }

    public enum TestPlanStatus
    {
        Draft,
        InProgress,
        Completed
    }

    public enum TestPlanOutcome
    {
        ReleaseReady,
        Rejected,
        Conditional
    }

    public enum TestRunStatus
    {
        Untested,
        Pass,
        Fail,
        Blocked,
        Skipped
    }

    public enum TestStepStatus
    {
        Pass,
        Fail,
        Skipped
    }
}
