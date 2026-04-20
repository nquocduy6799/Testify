using Testify.Shared.DTOs.Meetings;

namespace Testify.Interfaces
{
    public interface IMeetingSummaryService
    {
        Task<string> GenerateSummaryAsync(int meetingId);
    }
}
