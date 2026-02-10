using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Chat
{
    public class PinMessageRequest
    {
        [Required]
        public int MessageId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
