using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Chat
{
    public class UpdateRoomRequest
    {
        [StringLength(200, MinimumLength = 1)]
        public string? RoomName { get; set; }
    }

    public class AddMembersRequest
    {
        [Required]
        public List<string> UserIds { get; set; } = new();
    }
}
