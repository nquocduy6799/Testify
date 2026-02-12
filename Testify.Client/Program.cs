using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Testify.Client.Features.Invitations.Services;
using Testify.Client.Features.Kanban.Services;
using Testify.Client.Features.Milestones.Services;
using Testify.Client.Features.Notifications.Services;
using Testify.Client.Features.Projects.Services;
using Testify.Client.Interfaces;
using Testify.Client.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Register services here
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IKanbanTaskService, KanbanTaskService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddScoped<IChatService, Testify.Client.Features.Chat.Services.ChatService>();


// Register SignalR for real-time notifications
builder.Services.AddScoped<NotificationHubService>();
builder.Services.AddScoped<Testify.Client.Features.Chat.Services.ChatHubService>();
builder.Services.AddScoped<Testify.Client.Features.Chat.Services.CallHubService>();
builder.Services.AddScoped<Testify.Client.Features.Chat.Services.WebRtcService>();

await builder.Build().RunAsync();
