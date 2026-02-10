using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Chat
{
    public class AddReactionRequest
    {
        [Required]
        public int MessageId { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Emoji { get; set; } = string.Empty;
    }
}
