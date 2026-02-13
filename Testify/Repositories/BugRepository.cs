using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;
using Testify.Interfaces;
using Testify.Interfaces.Testify.Interfaces;
using Testify.Shared.DTOs.Bugs;
using Testify.Shared.Enums;
using static Testify.Shared.Enums.MilestoneEnum;

namespace Testify.Repositories
{
    public class BugRepository : IBugRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskActivityRepository _taskActivityRepository;

        public BugRepository(ApplicationDbContext context, ITaskActivityRepository taskActivityRepository)
        {
            _context = context;
            _taskActivityRepository = taskActivityRepository;
        }

        public async Task<BugResponse> CreateBugFromTestRunAsync(CreateBugFromTestRunRequest request, string userName, string userId)
        {
            // Create the bug as a KanbanTask
            var bug = new KanbanTask
            {
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = KanbanTaskStatus.ToDo,
                Priority = request.Priority,
                AssigneeId = request.AssigneeId,
                Type = TaskType.Bug // This is the key: mark it as a bug
            };

            bug.MarkAsCreated(userName);
            _context.KanbanTasks.Add(bug);
            await _context.SaveChangesAsync();

            // Link the bug to the failed test run steps
            if (request.FailedRunStepIds != null && request.FailedRunStepIds.Any())
            {
                foreach (var runStepId in request.FailedRunStepIds)
                {
                    var linkedRunStep = new TaskLinkedRunStep
                    {
                        TaskId = bug.Id,
                        RunStepId = runStepId,
                        LinkedAt = DateTime.UtcNow,
                    };
                    _context.TaskLinkedRunSteps.Add(linkedRunStep);
                }
                await _context.SaveChangesAsync();
            }

            // Record activity
            await _taskActivityRepository.RecordTaskCreationAsync(bug, userName, userId);

            // Return the created bug with full context
            return await GetBugByIdAsync(bug.Id)
                   ?? throw new InvalidOperationException("Failed to retrieve created bug.");
        }

        public async Task<BugResponse?> GetBugByIdAsync(int bugId)
        {
            var bug = await _context.KanbanTasks
                .Where(t => t.Id == bugId && t.Type == TaskType.Bug && !t.IsDeleted)
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync();

            return bug != null ? await MapToBugResponseAsync(bug) : null;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByProjectIdAsync(int projectId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.Milestone.ProjectId == projectId
                            && t.Type == TaskType.Bug
                            && !t.IsDeleted
                            && t.Milestone.Status == MilestoneStatus.Active)
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByMilestoneIdAsync(int milestoneId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.MilestoneId == milestoneId && t.Type == TaskType.Bug && !t.IsDeleted)
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByTestCaseIdAsync(int testCaseId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.Type == TaskType.Bug
                            && !t.IsDeleted
                            && t.LinkedRunSteps.Any(lrs => lrs.RunStep.Run.TestCaseId == testCaseId))
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByTestRunIdAsync(int testRunId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.Type == TaskType.Bug
                            && !t.IsDeleted
                            && t.LinkedRunSteps.Any(lrs => lrs.RunStep.RunId == testRunId))
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByAssigneeIdAsync(string assigneeId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.AssigneeId == assigneeId && t.Type == TaskType.Bug && !t.IsDeleted)
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<IEnumerable<BugResponse>> GetBugsByReporterIdAsync(string reporterId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.CreatedBy == reporterId && t.Type == TaskType.Bug && !t.IsDeleted)
                .Include(t => t.Milestone)
                    .ThenInclude(m => m.Project)
                .Include(t => t.Assignee)
                .Include(t => t.LinkedRunSteps)
                    .ThenInclude(lrs => lrs.RunStep)
                        .ThenInclude(rs => rs.Run)
                            .ThenInclude(r => r.TestCase)
                .Include(t => t.Attachments)
                .ToListAsync();

            var bugResponses = new List<BugResponse>();
            foreach (var bug in bugs)
            {
                bugResponses.Add(await MapToBugResponseAsync(bug));
            }
            return bugResponses;
        }

        public async Task<bool> LinkRunStepToBugAsync(LinkRunStepToBugRequest request, string userName)
        {
            var bug = await _context.KanbanTasks.FindAsync(request.BugId);
            if (bug == null || bug.Type != TaskType.Bug || bug.IsDeleted)
                return false;

            var runStep = await _context.TestRunSteps.FindAsync(request.RunStepId);
            if (runStep == null)
                return false;

            // Check if already linked
            var existingLink = await _context.TaskLinkedRunSteps
                .AnyAsync(lrs => lrs.TaskId == request.BugId && lrs.RunStepId == request.RunStepId);

            if (existingLink)
                return false;

            var linkedRunStep = new TaskLinkedRunStep
            {
                TaskId = request.BugId,
                RunStepId = request.RunStepId,
                LinkedAt = DateTime.UtcNow,
            };

            _context.TaskLinkedRunSteps.Add(linkedRunStep);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnlinkRunStepFromBugAsync(int bugId, int runStepId)
        {
            var link = await _context.TaskLinkedRunSteps
                .FirstOrDefaultAsync(lrs => lrs.TaskId == bugId && lrs.RunStepId == runStepId);

            if (link == null)
                return false;

            _context.TaskLinkedRunSteps.Remove(link);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BugSummary> GetBugSummaryByProjectIdAsync(int projectId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.Milestone.ProjectId == projectId
                            && t.Type == TaskType.Bug
                            && !t.IsDeleted
                            && t.Milestone.Status == MilestoneStatus.Active)
                .ToListAsync();

            return GenerateBugSummary(bugs);
        }

        public async Task<BugSummary> GetBugSummaryByMilestoneIdAsync(int milestoneId)
        {
            var bugs = await _context.KanbanTasks
                .Where(t => t.MilestoneId == milestoneId && t.Type == TaskType.Bug && !t.IsDeleted)
                .ToListAsync();

            return GenerateBugSummary(bugs);
        }

        public async Task<bool> UpdateBugStatusAsync(int bugId, KanbanTaskStatus status, string userName, string userId)
        {
            var bug = await _context.KanbanTasks.FindAsync(bugId);

            if (bug == null || bug.Type != TaskType.Bug || bug.IsDeleted)
                return false;

            var originalStatus = bug.Status;
            bug.Status = status;
            bug.MarkAsUpdated(userName);

            try
            {
                await _context.SaveChangesAsync();

                // Record status change activity
                var activity = new TaskActivity
                {
                    KanbanTaskId = bugId,
                    FullName = userName,
                    Action = "Status Changed",
                    OldValue = originalStatus.ToString(),
                    NewValue = status.ToString(),
                    Description = $"Bug status changed from {originalStatus} to {status}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _taskActivityRepository.CreateActivityAsync(activity);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        // Helper method to map KanbanTask to BugResponse
        private async Task<BugResponse> MapToBugResponseAsync(KanbanTask bug)
        {
            // Get the first linked test run (if any) for context
            var firstLinkedRunStep = bug.LinkedRunSteps.FirstOrDefault();
            var testCaseId = firstLinkedRunStep?.RunStep?.Run?.TestCaseId;
            var testCaseName = firstLinkedRunStep?.RunStep?.Run?.TestCase?.Title;
            var testRunId = firstLinkedRunStep?.RunStep?.RunId;

            // Get reporter info (the person who created the bug)
            ApplicationUser? reporter = null;
            if (!string.IsNullOrEmpty(bug.CreatedBy))
            {
                reporter = await _context.Users.FindAsync(bug.CreatedBy);
            }

            var response = new BugResponse
            {
                Id = bug.Id,
                MilestoneId = bug.MilestoneId,
                MilestoneName = bug.Milestone?.Name ?? string.Empty,
                ProjectId = bug.Milestone?.ProjectId ?? 0,
                ProjectName = bug.Milestone?.Project?.Name ?? string.Empty,
                Title = bug.Title,
                Description = bug.Description,
                StepsToReproduce = GenerateStepsToReproduce(bug.LinkedRunSteps),
                DueDate = bug.DueDate,
                Status = bug.Status,
                Priority = bug.Priority,
                AssigneeId = bug.AssigneeId,
                AssigneeName = bug.Assignee?.FullName,
                AssigneeAvatarUrl = bug.Assignee?.AvatarUrl,
                TestCaseId = testCaseId,
                TestCaseName = testCaseName,
                TestRunId = testRunId,
                ReportedBy = bug.CreatedBy,
                ReportedByName = reporter?.FullName,
                LinkedRunSteps = bug.LinkedRunSteps.Select(lrs => new LinkedRunStepInfo
                {
                    RunStepId = lrs.RunStepId,
                    StepNumber = lrs.RunStep?.StepNumber ?? 0,
                    Action = lrs.RunStep?.Action ?? string.Empty,
                    ExpectedResult = lrs.RunStep?.ExpectedResult ?? string.Empty,
                    ActualResult = lrs.RunStep?.ActualResult,
                    TestData = lrs.RunStep?.TestData,
                    LinkedAt = lrs.LinkedAt
                }).ToList(),
                Attachments = bug.Attachments.Select(a => new TestRunStepAttachmentResponse
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                }).ToList(),
                CreatedAt = bug.CreatedAt,
                UpdatedAt = bug.UpdatedAt,
                ResolvedAt = bug.Status == KanbanTaskStatus.Done ? bug.UpdatedAt : null
            };

            return response;
        }

        // Helper method to generate "Steps to Reproduce" from linked run steps
        private string GenerateStepsToReproduce(ICollection<TaskLinkedRunStep> linkedRunSteps)
        {
            if (linkedRunSteps == null || !linkedRunSteps.Any())
                return string.Empty;

            var steps = new System.Text.StringBuilder();
            steps.AppendLine("Steps to Reproduce:");
            steps.AppendLine();

            foreach (var linkedStep in linkedRunSteps.OrderBy(lrs => lrs.RunStep?.StepNumber))
            {
                var runStep = linkedStep.RunStep;
                if (runStep != null)
                {
                    steps.AppendLine($"Step {runStep.StepNumber}: {runStep.Action}");

                    if (!string.IsNullOrWhiteSpace(runStep.TestData))
                        steps.AppendLine($"  Test Data: {runStep.TestData}");

                    steps.AppendLine($"  Expected Result: {runStep.ExpectedResult}");

                    if (!string.IsNullOrWhiteSpace(runStep.ActualResult))
                        steps.AppendLine($"  Actual Result: {runStep.ActualResult}");

                    steps.AppendLine();
                }
            }

            return steps.ToString();
        }

        // Helper method to generate bug summary statistics
        private BugSummary GenerateBugSummary(List<KanbanTask> bugs)
        {
            var summary = new BugSummary
            {
                TotalBugs = bugs.Count,
                OpenBugs = bugs.Count(b => b.Status == KanbanTaskStatus.ToDo),
                InProgressBugs = bugs.Count(b => b.Status == KanbanTaskStatus.InProgress),
                ResolvedBugs = bugs.Count(b => b.Status == KanbanTaskStatus.Done),
                CriticalBugs = bugs.Count(b => b.Priority == TaskPriority.High), 
                HighPriorityBugs = bugs.Count(b => b.Priority == TaskPriority.High),
                BugsByStatus = bugs.GroupBy(b => b.Status.ToString())
                                  .ToDictionary(g => g.Key, g => g.Count()),
                BugsByPriority = bugs.GroupBy(b => b.Priority.ToString())
                                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return summary;
        }
    }
}