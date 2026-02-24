using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    public class SendMessageRequest
    {
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;
        
        public MessageType MessageType { get; set; } = MessageType.Text;
        
        public int? ParentMessageId { get; set; }
    }
}
