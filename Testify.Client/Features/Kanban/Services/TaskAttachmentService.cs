using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TaskAttachments;

namespace Testify.Client.Features.Kanban.Services
{
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/taskattachments";

        public TaskAttachmentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskAttachmentResponse>> UploadAttachmentsAsync(int kanbanTaskId, List<IBrowserFile> files)
        {
            if (files == null || files.Count == 0)
                return new List<TaskAttachmentResponse>();

            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024)); // 10MB
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "files", file.Name);
            }

            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/upload/{kanbanTaskId}", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<TaskAttachmentResponse>>();
                return result ?? new List<TaskAttachmentResponse>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to upload attachments.", ex);
            }
        }

        public async Task<List<TaskAttachmentResponse>> GetAttachmentsByTaskIdAsync(int kanbanTaskId)
        {
            try
            {
                var attachments = await _httpClient.GetFromJsonAsync<List<TaskAttachmentResponse>>($"{ApiEndpoint}/task/{kanbanTaskId}");
                return attachments ?? new List<TaskAttachmentResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TaskAttachmentResponse>();
            }
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{attachmentId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}