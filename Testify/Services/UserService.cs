using Microsoft.AspNetCore.Identity;
using Testify.Client.Interfaces;
using Testify.Data;
using Testify.Shared.DTOs.Users;

namespace Testify.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<UserListItemResponse>> GetUsersAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserListItemResponse>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserListItemResponse
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PrimaryRole = roles.FirstOrDefault() ?? "User"
            });
        }
        return result;
    }

    public async Task<UserResponse> GetUserByIdAsync(string userId)
    {
        var u = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");
        var roles = await _userManager.GetRolesAsync(u);
        return MapToResponse(u, roles);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AvatarUrl = request.AvatarUrl,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (request.Roles?.Any() == true)
            await _userManager.AddToRolesAsync(user, request.Roles.Distinct());

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new Exception("User not found");

        if (request.Email != null) { user.Email = request.Email; user.UserName = request.Email; }
        if (request.FullName != null) user.FullName = request.FullName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
        }

        await _userManager.UpdateAsync(user);

        if (request.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, request.Roles.Distinct());
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<List<string>> GetAvailableRolesAsync()
    {
        return _roleManager.Roles.Select(r => r.Name!).ToList();
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task<bool> AssignRolesAsync(AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return false;
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRolesAsync(user, request.Roles.Distinct());
        return result.Succeeded;
    }

    public async Task<List<UserListItemResponse>> SearchUsersAsync(string searchTerm)
    {
        var lower = searchTerm.ToLower();
        var users = _userManager.Users
            .Where(u => (u.Email != null && u.Email.ToLower().Contains(lower)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(lower)))
            .ToList();

        var result = new List<UserListItemResponse>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserListItemResponse
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PrimaryRole = roles.FirstOrDefault() ?? "User"
            });
        }
        return result;
    }

    public async Task<List<UserListItemResponse>> GetUsersByRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var result = new List<UserListItemResponse>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserListItemResponse
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PrimaryRole = roles.FirstOrDefault() ?? "User"
            });
        }
        return result;
    }

    public async Task<List<UserListItemResponse>> GetAvailableUsersForProjectAsync(int projectId)
        => await GetUsersAsync();

    private static UserResponse MapToResponse(ApplicationUser u, IList<string> roles) => new()
    {
        Id = u.Id,
        Email = u.Email ?? "",
        FullName = u.FullName,
        AvatarUrl = u.AvatarUrl,
        PhoneNumber = u.PhoneNumber,
        EmailConfirmed = u.EmailConfirmed,
        CreatedAt = DateTime.UtcNow,
        IsActive = u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow,
        Roles = roles.ToList()
    };
}
