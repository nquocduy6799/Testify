using Testify.Shared.DTOs.Users;

namespace Testify.Client.Interfaces
{
    public interface IUserService
    {
        Task<List<UserListItemResponse>> GetUsersAsync();

        Task<UserResponse> GetUserByIdAsync(string userId);

        Task<UserResponse> CreateUserAsync(CreateUserRequest request);

        Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);

        Task<bool> DeleteUserAsync(string userId);



        Task<List<string>> GetAvailableRolesAsync();

        Task<List<string>> GetUserRolesAsync(string userId);

        Task<bool> AssignRolesAsync(AssignRoleRequest request);

        Task<List<UserListItemResponse>> SearchUsersAsync(string searchTerm);

        Task<List<UserListItemResponse>> GetUsersByRoleAsync(string roleName);

        Task<List<UserListItemResponse>> GetAvailableUsersForProjectAsync(int projectId);
    }
}
