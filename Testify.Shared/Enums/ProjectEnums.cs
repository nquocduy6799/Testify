using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Enums
{
    public enum ProjectRole
    {
        PM = 0,
        Tester = 1,
        Dev = 2
    }

    public enum KanbanTaskStatus
    {
        ToDo = 0,
        InProgress = 1,
        Testing = 2,
        Done = 3,
        Cancelled = 4
    }

    public enum TaskType
    {
        Feature = 0,
        Bug = 1
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
}
