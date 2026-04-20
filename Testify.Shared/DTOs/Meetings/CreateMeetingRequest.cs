using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Meetings
{
    public class CreateMeetingRequest
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Range(5, 60)]
        public int MaxDurationMinutes { get; set; } = 20;
    }
}
