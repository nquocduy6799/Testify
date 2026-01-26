using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Client.Features.Projects.Services
{
    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/projects";

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProjectResponse>> GetProjectsAsync()
        {
            try
            {
                var projects = await _httpClient.GetFromJsonAsync<List<ProjectResponse>>(ApiEndpoint);
                return projects ?? new List<ProjectResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<ProjectResponse>();
            }
        }

        public async Task<ProjectResponse> GetProjectByIdAsync(int id)
        {
            try
            {
                var project = await _httpClient.GetFromJsonAsync<ProjectResponse>($"{ApiEndpoint}/{id}");
                return project ?? throw new InvalidOperationException($"Project with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve project with ID {id}.", ex);
            }
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var createdProject = await response.Content.ReadFromJsonAsync<ProjectResponse>();
                return createdProject ?? throw new InvalidOperationException("Failed to create project.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create project.", ex);
            }
        }

        public async Task<ProjectResponse> UpdateProjectAsync(int id, UpdateProjectRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();

                // Since PUT returns NoContent (204), fetch the updated project
                return await GetProjectByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update project with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<ProjectRole?> GetMyRoleInProjectAsync(int projectId)
        {
            try
            {
                var role = await _httpClient.GetFromJsonAsync<ProjectRole?>($"{ApiEndpoint}/{projectId}/my-role");
                return role;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<TeamMemberResponse>> GetMembersAsync(int projectId)
        {
            try
            {
                var members = await _httpClient.GetFromJsonAsync<List<TeamMemberResponse>>($"{ApiEndpoint}/{projectId}/members");
                return members ?? new List<TeamMemberResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TeamMemberResponse>();
            }
        }

        public async Task<bool> RemoveMemberAsync(int projectId, int memberId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{projectId}/members/{memberId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}