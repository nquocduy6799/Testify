using Testify.Shared.DTOs.Meetings;

namespace Testify.Client.Features.Meetings.Services
{
    public interface IMeetingService
    {
        Task<MeetingResponse?> CreateMeetingAsync(CreateMeetingRequest request);
        Task<MeetingResponse?> GetMeetingAsync(int meetingId);
        Task<List<MeetingResponse>> GetProjectMeetingsAsync(int projectId);
        Task<MeetingResponse?> StartMeetingAsync(int meetingId);
        Task<MeetingResponse?> EndMeetingAsync(int meetingId);
        Task<bool> JoinMeetingAsync(int meetingId);
        Task<bool> LeaveMeetingAsync(int meetingId);
        Task<List<MeetingTranscriptEntry>> GetTranscriptsAsync(int meetingId);
        Task<bool> AddTranscriptAsync(int meetingId, string content);
    }
}
