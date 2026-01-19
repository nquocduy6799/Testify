using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Testify.Entities;

namespace Testify.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        // Project Management
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; }
        public DbSet<ProjectFolder> ProjectFolders { get; set; }

        // Milestone & Tasks
        public DbSet<Milestone> Milestones { get; set; }

        public DbSet<KanbanTask> Tasks { get; set; }

        // Templates
        public DbSet<TestSuiteTemplate> TestSuiteTemplates { get; set; }
        public DbSet<TestCaseTemplate> TestCaseTemplates { get; set; }
        public DbSet<TestStepTemplate> TestStepTemplates { get; set; }

        // Test Suites & Cases
        public DbSet<TestSuite> TestSuites { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<TestStep> TestSteps { get; set; }

        // Test Plans & Runs
        public DbSet<TestPlan> TestPlans { get; set; }
        public DbSet<TestPlanSuite> TestPlanSuites { get; set; }
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<TestRunStep> TestRunSteps { get; set; }
        public DbSet<TaskLinkedRunStep> TaskLinkedRunSteps { get; set; }
        public DbSet<TestRunStepAttachment> TestRunStepAttachments { get; set; }

        // Chat System
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomParticipant> ChatRoomParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatMessageAttachment> ChatMessageAttachments { get; set; }
        public DbSet<ChatMessageReaction> ChatMessageReactions { get; set; }
        public DbSet<ChatMessageRead> ChatMessageReads { get; set; }
        public DbSet<ChatPinnedMessage> ChatPinnedMessages { get; set; }
        public DbSet<ChatNotification> ChatNotifications { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }
    }
}
