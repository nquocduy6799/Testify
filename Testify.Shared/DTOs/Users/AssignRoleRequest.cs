using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Users;

public class AssignRoleRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one role must be assigned")]
    public List<string> Roles { get; set; } = new();
}