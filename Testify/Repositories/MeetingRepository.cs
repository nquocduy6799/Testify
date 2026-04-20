using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Shared.DTOs.Meetings;
using Testify.Shared.Enums;
using Testify.Shared.Helpers;

namespace Testify.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPresenceService _presence;

        public MeetingRepository(ApplicationDbContext context, IUserPresenceService presence)
        {
            _context = context;
            _presence = presence;
        }

        public async Task<MeetingResponse> CreateMeetingAsync(CreateMeetingRequest request, string hostUserId)
        {
            var project = await _context.Projects
                .Include(p => p.TeamMembers)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted)
                ?? throw new InvalidOperationException("Project not found.");

            var meeting = new Meeting
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                HostUserId = hostUserId,
                Status = MeetingStatus.Scheduled,
                MaxDurationMinutes = request.MaxDurationMinutes
            };
            meeting.MarkAsCreated(hostUserId);

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Add all project team members as participants
            foreach (var member in project.TeamMembers)
            {
                var participant = new MeetingParticipant
                {
                    MeetingId = meeting.Id,
                    UserId = member.UserId,
                    HasAttended = false
                };
                _context.MeetingParticipants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return await GetMeetingByIdAsync(meeting.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created meeting.");
        }

        public async Task<MeetingResponse?> GetMeetingByIdAsync(int meetingId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Project)
                .Include(m => m.Host)
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null) return null;

            return MapToResponse(meeting);
        }

        public async Task<List<MeetingResponse>> GetProjectMeetingsAsync(int projectId)
        {
            var meetings = await _context.Meetings
                .Include(m => m.Project)
                .Include(m => m.Host)
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Where(m => m.ProjectId == projectId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

            return meetings.Select(MapToResponse).ToList();
        }

        public async Task<MeetingResponse?> StartMeetingAsync(int meetingId, string userId)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null || meeting.HostUserId != userId) return null;
            if (meeting.Status != MeetingStatus.Scheduled) return null;

            meeting.Status = MeetingStatus.InProgress;
            meeting.StartedAt = DateTimeHelper.GetVietnamTime();
            meeting.MarkAsUpdated(userId);

            await _context.SaveChangesAsync();

            return await GetMeetingByIdAsync(meetingId);
        }

        public async Task<MeetingResponse?> EndMeetingAsync(int meetingId, string userId)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null || meeting.HostUserId != userId) return null;
            if (meeting.Status != MeetingStatus.InProgress) return null;

            meeting.Status = MeetingStatus.Ended;
            meeting.EndedAt = DateTimeHelper.GetVietnamTime();
            meeting.MarkAsUpdated(userId);

            await _context.SaveChangesAsync();

            return await GetMeetingByIdAsync(meetingId);
        }

        public async Task<bool> JoinMeetingAsync(int meetingId, string userId)
        {
            var participant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(p => p.MeetingId == meetingId && p.UserId == userId);

            if (participant == null) return false;

            participant.JoinedAt = DateTimeHelper.GetVietnamTime();
            participant.HasAttended = true;
            participant.LeftAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveMeetingAsync(int meetingId, string userId)
        {
            var participant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(p => p.MeetingId == meetingId && p.UserId == userId);

            if (participant == null) return false;

            participant.LeftAt = DateTimeHelper.GetVietnamTime();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MeetingTranscriptEntry> AddTranscriptAsync(int meetingId, string userId, string content)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var transcript = new MeetingTranscript
            {
                MeetingId = meetingId,
                UserId = userId,
                Content = content,
                Timestamp = DateTimeHelper.GetVietnamTime()
            };

            _context.MeetingTranscripts.Add(transcript);
            await _context.SaveChangesAsync();

            return new MeetingTranscriptEntry
            {
                Id = transcript.Id,
                UserId = userId,
                UserName = user.FullName ?? user.UserName ?? "Unknown",
                AvatarUrl = user.AvatarUrl,
                Content = content,
                Timestamp = transcript.Timestamp
            };
        }

        public async Task<List<MeetingTranscriptEntry>> GetTranscriptsAsync(int meetingId)
        {
            return await _context.MeetingTranscripts
                .Include(t => t.User)
                .Where(t => t.MeetingId == meetingId)
                .OrderBy(t => t.Timestamp)
                .Select(t => new MeetingTranscriptEntry
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.User.FullName ?? t.User.UserName ?? "Unknown",
                    AvatarUrl = t.User.AvatarUrl,
                    Content = t.Content,
                    Timestamp = t.Timestamp
                })
                .ToListAsync();
        }

        public async Task<bool> SaveSummaryAsync(int meetingId, string summaryContent)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null) return false;

            meeting.SummaryContent = summaryContent;
            meeting.SummaryGeneratedAt = DateTimeHelper.GetVietnamTime();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MeetingSummaryResponse?> GetSummaryAsync(int meetingId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Project)
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null || string.IsNullOrEmpty(meeting.SummaryContent)) return null;

            return new MeetingSummaryResponse
            {
                MeetingId = meeting.Id,
                Title = meeting.Title,
                ProjectName = meeting.Project?.Name ?? "",
                Summary = meeting.SummaryContent,
                GeneratedAt = meeting.SummaryGeneratedAt,
                StartedAt = meeting.StartedAt,
                EndedAt = meeting.EndedAt,
                ParticipantCount = meeting.Participants.Count,
                AttendedCount = meeting.Participants.Count(p => p.HasAttended)
            };
        }

        public async Task<bool> IsUserInProjectAsync(int projectId, string userId)
        {
            return await _context.ProjectTeamMembers
                .AnyAsync(ptm => ptm.ProjectId == projectId && ptm.UserId == userId);
        }

        public async Task<bool> IsHostAsync(int meetingId, string userId)
        {
            return await _context.Meetings
                .AnyAsync(m => m.Id == meetingId && m.HostUserId == userId && !m.IsDeleted);
        }

        public async Task<List<string>> GetNonAttendedUserIdsAsync(int meetingId)
        {
            return await _context.MeetingParticipants
                .Where(p => p.MeetingId == meetingId && !p.HasAttended)
                .Select(p => p.UserId)
                .ToListAsync();
        }

        public async Task<List<string>> GetAttendedUserIdsAsync(int meetingId)
        {
            return await _context.MeetingParticipants
                .Where(p => p.MeetingId == meetingId && p.HasAttended)
                .Select(p => p.UserId)
                .ToListAsync();
        }

        private MeetingResponse MapToResponse(Meeting meeting)
        {
            return new MeetingResponse
            {
                Id = meeting.Id,
                ProjectId = meeting.ProjectId,
                ProjectName = meeting.Project?.Name ?? "",
                Title = meeting.Title,
                HostUserId = meeting.HostUserId,
                HostName = meeting.Host?.FullName ?? meeting.Host?.UserName ?? "Unknown",
                HostAvatarUrl = meeting.Host?.AvatarUrl,
                Status = meeting.Status,
                MaxDurationMinutes = meeting.MaxDurationMinutes,
                StartedAt = meeting.StartedAt,
                EndedAt = meeting.EndedAt,
                HasSummary = !string.IsNullOrEmpty(meeting.SummaryContent),
                CreatedAt = meeting.CreatedAt,
                Participants = meeting.Participants.Select(p => new MeetingParticipantResponse
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User?.UserName ?? "Unknown",
                    FullName = p.User?.FullName,
                    AvatarUrl = p.User?.AvatarUrl,
                    JoinedAt = p.JoinedAt,
                    LeftAt = p.LeftAt,
                    HasAttended = p.HasAttended,
                    IsOnline = _presence.IsOnline(p.UserId)
                }).ToList()
            };
        }
    }
}
