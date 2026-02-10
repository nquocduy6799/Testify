using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Testify.Shared.Enums;

namespace Testify.Shared.DTOs.Chat
{
    public class CreateChatRoomRequest
    {
        [StringLength(200)]
        public string? RoomName { get; set; }
        
        [Required]
        public ChatRoomType RoomType { get; set; }
        
        public int? ProjectId { get; set; }
        
        // For direct messages: UserId of the other person
        public string? OtherUserId { get; set; }
        
        // For group chats: List of participant UserIds
        public List<string> ParticipantUserIds { get; set; } = new();
    }
}
