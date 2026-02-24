using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Chat
{
    public class MarkAsReadRequest
    {
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public int MessageId { get; set; }
    }
}
