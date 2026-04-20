using System.Net.Http.Json;
using Testify.Shared.DTOs.Meetings;

namespace Testify.Client.Features.Meetings.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/meetings";

        public MeetingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MeetingResponse?> CreateMeetingAsync(CreateMeetingRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MeetingResponse>();
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<MeetingResponse?> GetMeetingAsync(int meetingId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MeetingResponse>($"{ApiEndpoint}/{meetingId}");
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<List<MeetingResponse>> GetProjectMeetingsAsync(int projectId)
        {
            try
            {
                var meetings = await _httpClient.GetFromJsonAsync<List<MeetingResponse>>($"{ApiEndpoint}/project/{projectId}");
                return meetings ?? new List<MeetingResponse>();
            }
            catch (HttpRequestException) { return new List<MeetingResponse>(); }
        }

        public async Task<MeetingResponse?> StartMeetingAsync(int meetingId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{meetingId}/start", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MeetingResponse>();
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<MeetingResponse?> EndMeetingAsync(int meetingId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{meetingId}/end", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MeetingResponse>();
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<bool> JoinMeetingAsync(int meetingId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{meetingId}/join", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        public async Task<bool> LeaveMeetingAsync(int meetingId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{ApiEndpoint}/{meetingId}/leave", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        public async Task<List<MeetingTranscriptEntry>> GetTranscriptsAsync(int meetingId)
        {
            try
            {
                var transcripts = await _httpClient.GetFromJsonAsync<List<MeetingTranscriptEntry>>($"{ApiEndpoint}/{meetingId}/transcripts");
                return transcripts ?? new List<MeetingTranscriptEntry>();
            }
            catch (HttpRequestException) { return new List<MeetingTranscriptEntry>(); }
        }

        public async Task<bool> AddTranscriptAsync(int meetingId, string content)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/{meetingId}/transcripts", new { Content = content });
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }
    }
}
