using System;
using System.Collections.Generic;
using System.Text;

namespace Testify.Entities
{
    public class TestPlanSuite
    {
        public int Id { get; set; }
        public int TestPlanId { get; set; }
        public int TestSuiteId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual TestPlan TestPlan { get; set; } = null!;
        public virtual TestSuite TestSuite { get; set; } = null!;
    }
}
