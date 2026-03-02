using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Users;

namespace Testify.Client.Features.Admin.Users.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "/api/users";

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ============ CRUD Operations ============

        public async Task<List<UserListItemResponse>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserListItemResponse>>(BaseUrl);
                return response ?? new List<UserListItemResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserResponse>($"{BaseUrl}/{userId}");
                return response ?? throw new Exception("User not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
                response.EnsureSuccessStatusCode();

                var createdUser = await response.Content.ReadFromJsonAsync<UserResponse>();
                return createdUser ?? throw new Exception("Failed to create user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{userId}", request);
                response.EnsureSuccessStatusCode();

                var updatedUser = await response.Content.ReadFromJsonAsync<UserResponse>();
                return updatedUser ?? throw new Exception("Failed to update user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user {userId}: {ex.Message}");
                return false;
            }
        }

        // ============ Profile Operations ============

        public async Task<UserProfileResponse> GetMyProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserProfileResponse>($"{BaseUrl}/me");
                return response ?? throw new Exception("Failed to load profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching my profile: {ex.Message}");
                throw;
            }
        }

        public async Task<UserProfileResponse> UpdateMyProfileAsync(UpdateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/me", request);
                response.EnsureSuccessStatusCode();

                var updatedProfile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
                return updatedProfile ?? throw new Exception("Failed to update profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating my profile: {ex.Message}");
                throw;
            }
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserProfileResponse>($"{BaseUrl}/{userId}/profile");
                return response ?? throw new Exception("Profile not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching profile for user {userId}: {ex.Message}");
                throw;
            }
        }

        // ============ Authentication Operations ============

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/change-password", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{userId}/reset-password", new { NewPassword = newPassword });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password for user {userId}: {ex.Message}");
                return false;
            }
        }

        // ============ Role Management ============

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/roles");
                return response ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching available roles: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/{userId}/roles");
                return response ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching roles for user {userId}: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> AssignRolesAsync(AssignRoleRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/assign-roles", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning roles: {ex.Message}");
                return false;
            }
        }

        // ============ Search & Filter ============

        public async Task<List<UserListItemResponse>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserListItemResponse>>($"{BaseUrl}/search?term={Uri.EscapeDataString(searchTerm)}");
                return response ?? new List<UserListItemResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
                return new List<UserListItemResponse>();
            }
        }

        public async Task<List<UserListItemResponse>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserListItemResponse>>($"{BaseUrl}/by-role/{roleName}");
                return response ?? new List<UserListItemResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users by role {roleName}: {ex.Message}");
                return new List<UserListItemResponse>();
            }
        }

        // ============ Project-Related ============

        public async Task<List<UserListItemResponse>> GetProjectMembersAsync(int projectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserListItemResponse>>($"/api/projects/{projectId}/members");
                return response ?? new List<UserListItemResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching members for project {projectId}: {ex.Message}");
                return new List<UserListItemResponse>();
            }
        }

        public async Task<List<UserListItemResponse>> GetAvailableUsersForProjectAsync(int projectId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserListItemResponse>>($"/api/projects/{projectId}/available-users");
                return response ?? new List<UserListItemResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching available users for project {projectId}: {ex.Message}");
                return new List<UserListItemResponse>();
            }
        }
    }
}