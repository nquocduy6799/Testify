using Testify.Shared.DTOs.Meetings;

namespace Testify.Interfaces
{
    public interface IMeetingRepository
    {
        Task<MeetingResponse> CreateMeetingAsync(CreateMeetingRequest request, string hostUserId);
        Task<MeetingResponse?> GetMeetingByIdAsync(int meetingId);
        Task<List<MeetingResponse>> GetProjectMeetingsAsync(int projectId);
        Task<MeetingResponse?> StartMeetingAsync(int meetingId, string userId);
        Task<MeetingResponse?> EndMeetingAsync(int meetingId, string userId);
        Task<bool> JoinMeetingAsync(int meetingId, string userId);
        Task<bool> LeaveMeetingAsync(int meetingId, string userId);
        Task<MeetingTranscriptEntry> AddTranscriptAsync(int meetingId, string userId, string content);
        Task<List<MeetingTranscriptEntry>> GetTranscriptsAsync(int meetingId);
        Task<bool> SaveSummaryAsync(int meetingId, string summaryContent);
        Task<MeetingSummaryResponse?> GetSummaryAsync(int meetingId);
        Task<bool> IsUserInProjectAsync(int projectId, string userId);
        Task<bool> IsHostAsync(int meetingId, string userId);
        Task<List<string>> GetNonAttendedUserIdsAsync(int meetingId);
        Task<List<string>> GetAttendedUserIdsAsync(int meetingId);
    }
}
