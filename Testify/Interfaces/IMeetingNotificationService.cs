using Testify.Shared.DTOs.Meetings;

namespace Testify.Interfaces
{
    public interface IMeetingNotificationService
    {
        Task NotifyMeetingCreatedAsync(MeetingResponse meeting);
        Task NotifyMeetingStartedAsync(MeetingResponse meeting);
        Task NotifyMeetingSummaryAsync(int meetingId);
    }
}
