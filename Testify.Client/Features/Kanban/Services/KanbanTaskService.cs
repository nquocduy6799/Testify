using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Client.Features.Kanban.Services
{
    public class KanbanTaskService : IKanbanTaskService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/kanbantasks";

        public KanbanTaskService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<KanbanTaskResponse>> GetTasksByMilestoneIdAsync(int milestoneId)
        {
            try
            {
                var tasks = await _httpClient.GetFromJsonAsync<List<KanbanTaskResponse>>($"{ApiEndpoint}/milestone/{milestoneId}");
                return tasks ?? new List<KanbanTaskResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<KanbanTaskResponse>();
            }
        }

        public async Task<KanbanTaskResponse> GetTaskByIdAsync(int id)
        {
            try
            {
                var task = await _httpClient.GetFromJsonAsync<KanbanTaskResponse>($"{ApiEndpoint}/{id}");
                return task ?? throw new InvalidOperationException($"Task with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve task with ID {id}.", ex);
            }
        }

        public async Task<KanbanTaskResponse> CreateTaskAsync(CreateKanbanTaskRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var createdTask = await response.Content.ReadFromJsonAsync<KanbanTaskResponse>();
                return createdTask ?? throw new InvalidOperationException("Failed to create task.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create task.", ex);
            }
        }

        public async Task<KanbanTaskResponse> UpdateTaskAsync(int id, UpdateKanbanTaskRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();

                // Since PUT returns NoContent (204), fetch the updated task
                return await GetTaskByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update task with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
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

        public async Task<List<KanbanTaskResponse>> GetTasksByProjectIdAsync(int projectId)
        {
            try
            {
                var tasks = await _httpClient.GetFromJsonAsync<List<KanbanTaskResponse>>($"{ApiEndpoint}/project/{projectId}");
                return tasks ?? new List<KanbanTaskResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<KanbanTaskResponse>();
            }
        }

        async Task<List<TaskActivityResponse>> IKanbanTaskService.GetTaskActivityResponsesAsync(int taskId)
        {
            try
            {
                var activities = await _httpClient.GetFromJsonAsync<List<TaskActivityResponse>>($"{ApiEndpoint}/activities/{taskId}");
                return activities ?? new List<TaskActivityResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TaskActivityResponse>();
            }
        }
    }
}