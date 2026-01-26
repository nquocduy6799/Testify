using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Notifications;

namespace Testify.Client.Features.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/notifications";

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<NotificationResponse>> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiEndpoint);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Notification API Response ({response.StatusCode}): {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<NotificationResponse>();
                }

                var notifications = System.Text.Json.JsonSerializer.Deserialize<List<NotificationResponse>>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                return notifications ?? new List<NotificationResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return new List<NotificationResponse>();
            }
        }

        public async Task<NotificationResponse?> GetNotificationByIdAsync(long id)
        {
            try
            {
                var notification = await _httpClient.GetFromJsonAsync<NotificationResponse>($"{ApiEndpoint}/{id}");
                return notification;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<bool> AcceptInvitationAsync(long notificationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{notificationId}/accept", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeclineInvitationAsync(long notificationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{notificationId}/decline", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
