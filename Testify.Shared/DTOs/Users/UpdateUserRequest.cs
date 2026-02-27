using System.ComponentModel.DataAnnotations;

namespace Testify.Shared.DTOs.Users;

public class UpdateUserRequest
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    public bool? IsActive { get; set; }

    public List<string>? Roles { get; set; }
}