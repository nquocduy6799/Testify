using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Testify.Client.Features.Account.Services;
using Testify.Client.Features.Chat.Services;
using Testify.Client.Features.Invitations.Services;
using Testify.Client.Features.Kanban.Services;
using Testify.Client.Features.Marketplace.Services;
using Testify.Client.Features.Meetings.Services;
using Testify.Client.Features.Milestones.Services;
using Testify.Client.Features.Notifications.Services;
using Testify.Client.Features.Projects.Services;
using Testify.Client.Features.TestPlans.Services;
using Testify.Client.Features.TestRuns.Services;
using Testify.Client.Features.TestSuites.Services;
using Testify.Client.Features.TestTemplates.Services;
using Testify.Client.Interfaces;
using Testify.Client.Shared.Services;
using Testify.Components;
using Testify.Components.Account;
using Testify.Configuration;
using Testify.Data;
using Testify.Hubs;
using Testify.Interfaces;
using Testify.Repositories;
using Testify.Services;
using Testify.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddSingleton<IAppEmailService, Testify.Services.SmtpEmailService>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentitySmtpEmailSender>();

// Add HttpClient for server-side rendering
builder.Services.AddScoped(sp =>
{
    var navigationManager =
        sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// Register repositories
builder.Services.AddScoped<ICurrentUserRepository, CurrentUserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
builder.Services.AddScoped<IKanbanTaskRepository, KanbanTaskRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITemplateFolderRepository, TemplateFolderRepository>();
builder.Services.AddScoped<ITestSuiteTemplateRepository, TestSuiteTemplateRepository>();
builder.Services.AddScoped<ITestCaseTemplateRepository, TestCaseTemplateRepository>();
builder.Services.AddScoped<ITestSuiteRepository, TestSuiteRepository>();
builder.Services.AddScoped<ITestCaseRepository, TestCaseRepository>();
builder.Services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
builder.Services.AddScoped<ITaskActivityRepository, TaskActivityRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IBugRepository, BugRepository>();
builder.Services.AddScoped<ICallSessionRepository, CallSessionRepository>();
builder.Services.AddScoped<ITestPlanRepository, TestPlanRepository>();
builder.Services.AddScoped<ITestPlanSuiteRepository, TestPlanSuiteRepository>();
builder.Services.AddScoped<ITestRunRepository, TestRunRepository>();
builder.Services.AddScoped<ITestRunStepAttachmentRepository, TestRunStepAttachmentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITemplateReviewRepository, TemplateReviewRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingNotificationService, Testify.Services.MeetingNotificationService>();

// Gemini AI configuration (test case generation only)
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<Testify.Interfaces.IAiTestCaseService, GeminiTestCaseService>();

// Hosted services
builder.Services.AddHostedService<StaleCallCleanupService>();

// File upload settings and storage
builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection(FileUploadSettings.SectionName));
builder.Services.AddSingleton<IFileStorageService, Testify.Services.LocalFileStorageService>();

// Register services for server-side
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IKanbanTaskService, KanbanTaskService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<INotificationService, ServerNotificationRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddScoped<ITemplateFolderService, TemplateFolderService>();
builder.Services.AddScoped<ITestSuiteTemplateService, TestSuiteTemplateService>();
builder.Services.AddScoped<ITestCaseTemplateService, TestCaseTemplateService>();
builder.Services.AddScoped<ITestSuiteService, TestSuiteService>();
builder.Services.AddScoped<ITestCaseService, TestCaseService>();
builder.Services.AddScoped<Testify.Client.Interfaces.IAiTestCaseService, AiTestCaseService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBugService, BugService>();
builder.Services.AddScoped<ITestPlanService, TestPlanService>();
builder.Services.AddScoped<ITestRunService, TestRunService>();
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
builder.Services.AddSingleton<Testify.Shared.Interfaces.ISystemSettingsService, Testify.Services.SystemSettingsService>();
builder.Services.AddScoped<Testify.Shared.Interfaces.IDashboardService, Testify.Services.DashboardService>();
builder.Services.AddScoped<Testify.Client.Interfaces.IUserService, Testify.Services.UserService>();
builder.Services.AddScoped<ITemplateReviewService, TemplateReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();

builder.Services.AddScoped<ChatHubService>();
builder.Services.AddScoped<MeetingHubService>();
builder.Services.AddScoped<ModalService>();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Centralized exception handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserPresenceService, UserPresenceService>();

var app = builder.Build();

// Seed users and roles
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        await RoleSeeder.SeedRolesAsync(services);
//        await UserSeeder.SeedUsersAsync(services);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}

// Seed users and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
        await UserSeeder.SeedUsersAsync(services);

        // Seed marketplace data
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await MarketplaceSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Testify API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Auth middleware must run before any middleware that checks context.User
app.UseAuthentication();
app.UseAuthorization();

// Maintenance Mode middleware – redirects non-admin users when maintenance is active
app.UseMiddleware<Testify.Middleware.MaintenanceMiddleware>();

app.UseAntiforgery();

app.MapStaticAssets();

// Map API controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<CallHub>("/hubs/call");
app.MapHub<MeetingHub>("/hubs/meeting");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Testify.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

















//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using Testify.Client.Features.Invitations.Services;
//using Testify.Client.Features.Kanban.Services;
//using Testify.Client.Features.Milestones.Services;
//using Testify.Client.Features.Notifications.Services;
//using Testify.Client.Features.Projects.Services;
//using Testify.Client.Interfaces;
//using Testify.Components;
//using Testify.Components.Account;
//using Testify.Data;
//using Testify.Interfaces;
//using Testify.Repositories;
//using Testify.Hubs;
//using Testify.Client.Features.TestTemplates.Services;
//using Testify.Configuration;
//using Testify.Client.Shared.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder
//    .Services.AddRazorComponents()
//    .AddInteractiveServerComponents()
//    .AddInteractiveWebAssemblyComponents()
//    .AddAuthenticationStateSerialization();

//builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddScoped<IdentityRedirectManager>();

//builder
//    .Services.AddAuthentication(options =>
//    {
//        options.DefaultScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//    })
//    .AddIdentityCookies();
//builder.Services.AddAuthorization();

//var connectionString =
//    builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString)
//);
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder
//    .Services.AddIdentityCore<ApplicationUser>(options =>
//    {
//        options.SignIn.RequireConfirmedAccount = false;
//        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
//    })
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddSignInManager()
//    .AddDefaultTokenProviders();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

//// Add HttpClient for server-side rendering
//builder.Services.AddScoped(sp =>
//{
//    var navigationManager =
//        sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
//    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
//});

//// Register repositories
//builder.Services.AddScoped<ICurrentUserRepository, CurrentUserRepository>();
//builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
//builder.Services.AddScoped<IKanbanTaskRepository, KanbanTaskRepository>();
//builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
//builder.Services.AddScoped<ITemplateFolderRepository, TemplateFolderRepository>();
//builder.Services.AddScoped<ITestSuiteTemplateRepository, TestSuiteTemplateRepository>();
//builder.Services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();


//// Register services for server-side
//builder.Services.AddScoped<IProjectService, ProjectService>();
//builder.Services.AddScoped<IKanbanTaskService, KanbanTaskService>();
//builder.Services.AddScoped<IMilestoneService, MilestoneService>();
//builder.Services.AddScoped<INotificationService, ServerNotificationRepository>();
//builder.Services.AddScoped<IInvitationService, InvitationService>();
//builder.Services.AddScoped<ITemplateFolderService, TemplateFolderService>();
//builder.Services.AddScoped<ITestSuiteTemplateService, TestSuiteTemplateService>();
//builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();



//// Add controllers for API endpoints
//builder.Services.AddControllers();

//// Add Swagger/OpenAPI
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerConfiguration();


//// Add SignalR for real-time notifications
//builder.Services.AddSignalR();

//var app = builder.Build();

//// Seed users and roles
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        await RoleSeeder.SeedRolesAsync(services);
//        await UserSeeder.SeedUsersAsync(services);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseWebAssemblyDebugging();
//    app.UseMigrationsEndPoint();

//    // Enable Swagger UI in development
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Testify API v1");
//        options.RoutePrefix = "swagger"; // Access at /swagger
//    });
//}
//else
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}
//app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();

//app.UseAntiforgery();

//app.MapStaticAssets();

//// Map API controllers
//app.MapControllers();

//// Map SignalR Hub
//app.MapHub<NotificationHub>("/hubs/notifications");

//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode()
//    .AddInteractiveWebAssemblyRenderMode()
//    .AddAdditionalAssemblies(typeof(Testify.Client._Imports).Assembly);

//// Add additional endpoints required by the Identity /Account Razor components.
//app.MapAdditionalIdentityEndpoints();

//app.Run();