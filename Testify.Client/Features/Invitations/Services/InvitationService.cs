using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Invitations;

namespace Testify.Client.Features.Invitations.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/invitations";

        public InvitationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<InvitationResponse> SendInvitationAsync(SendInvitationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/send", request);
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                {
                    return new InvitationResponse
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode} (Empty response)"
                    };
                }

                try
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<InvitationResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result != null)
                    {
                         // If API returned failure status but successfully deserialized a response object,
                         // we return that object (it likely contains the specific error message)
                        return result;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Response was not JSON (e.g. likely a plain text error from server middleware)
                    return new InvitationResponse
                    {
                        Success = false,
                        Message = $"Server error: {content}"
                    };
                }

                return new InvitationResponse
                {
                    Success = false,
                    Message = $"Request failed with status: {response.StatusCode}"
                };
            }
            catch (HttpRequestException ex)
            {
                return new InvitationResponse
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
        }

        public async Task<List<PendingInvitationResponse>> GetPendingInvitationsAsync(int projectId)
        {
            try
            {
                var invitations = await _httpClient.GetFromJsonAsync<List<PendingInvitationResponse>>($"{ApiEndpoint}/project/{projectId}/pending");
                return invitations ?? new List<PendingInvitationResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<PendingInvitationResponse>();
            }
        }

        public async Task<bool> RevokeInvitationAsync(long invitationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{invitationId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
