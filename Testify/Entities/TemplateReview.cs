using Testify.Data;
using Testify.Shared.Helpers;

namespace Testify.Entities
{
    public class TemplateReview
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public required string UserId { get; set; }
        public bool IsStarred { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        // Navigation properties
        public virtual TestSuiteTemplate Template { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
