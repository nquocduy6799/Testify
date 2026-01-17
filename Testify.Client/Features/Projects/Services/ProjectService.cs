using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Projects;

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
    }
}