namespace Testify.Shared.DTOs.Users;

/// <summary>
/// Lightweight DTO for user list display (Grid/Table views)
/// </summary>
public class UserListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string PrimaryRole { get; set; } = "User";
}