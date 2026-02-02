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

        public DbSet<KanbanTask> KanbanTasks { get; set; }

        // Templates
        public DbSet<TemplateFolder> TemplateFolders { get; set; }
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskActivity> TaskActivities { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Chat System - prevent cascade delete conflicts
            builder.Entity<ChatMessageReaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ChatMessageRead>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ChatMessage>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ChatPinnedMessage>()
                .HasOne(pm => pm.Room)
                .WithMany(r => r.PinnedMessages)
                .HasForeignKey(pm => pm.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            // Test Plans - prevent cascade delete conflicts
            builder.Entity<TestPlanSuite>()
                .HasOne(tps => tps.TestPlan)
                .WithMany(tp => tp.TestPlanSuites)
                .HasForeignKey(tps => tps.TestPlanId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TestRun>()
                .HasOne(tr => tr.TestPlan)
                .WithMany(p => p.TestRuns)
                .HasForeignKey(tr => tr.PlanId)
                .OnDelete(DeleteBehavior.NoAction);

            // TaskLinkedRunStep - prevent multiple cascade paths
            builder.Entity<TaskLinkedRunStep>()
                .HasOne(tlrs => tlrs.Task)
                .WithMany(t => t.LinkedRunSteps)
                .HasForeignKey(tlrs => tlrs.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskLinkedRunStep>()
                .HasOne(tlrs => tlrs.RunStep)
                .WithMany(rs => rs.LinkedTasks)
                .HasForeignKey(tlrs => tlrs.RunStepId)
                .OnDelete(DeleteBehavior.NoAction);

            // Notifications - prevent cascade delete conflicts
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Project)
                .WithMany()
                .HasForeignKey(n => n.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
