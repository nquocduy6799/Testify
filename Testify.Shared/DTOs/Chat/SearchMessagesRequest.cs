using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Chat
{
    public class SearchMessagesRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Query { get; set; } = string.Empty;

        public int Skip { get; set; } = 0;

        [Range(1, 50)]
        public int Take { get; set; } = 20;
    }
}
