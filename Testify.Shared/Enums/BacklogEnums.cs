using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Shared.Enums
{
    public enum BacklogItemType
    {
        Epic = 0,
        UserStory = 1,
        Feature = 2,
        Bug = 3,
        Task = 4
    }

    public enum BacklogItemStatus
    {
        New = 0,
        Ready = 1,
        InProgress = 2,
        Done = 3,
        Cancelled = 4
    }

    public enum BacklogItemPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
