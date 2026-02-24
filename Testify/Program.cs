using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Testify.Client.Features.Invitations.Services;
using Testify.Client.Features.Kanban.Services;
using Testify.Client.Features.Milestones.Services;
using Testify.Client.Features.Notifications.Services;
using Testify.Client.Features.Projects.Services;
using Testify.Client.Interfaces;
using Testify.Components;
using Testify.Components.Account;
using Testify.Data;
using Testify.Interfaces;
using Testify.Repositories;
using Testify.Hubs;
using Testify.Client.Features.TestTemplates.Services;

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

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

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
builder.Services.AddScoped<IKanbanTaskRepository, KanbanTaskRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITemplateFolderRepository, TemplateFolderRepository>();
builder.Services.AddScoped<ITestSuiteTemplateRepository, TestSuiteTemplateRepository>();


// Register services for server-side
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IKanbanTaskService, KanbanTaskService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<INotificationService, ServerNotificationRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<ITemplateFolderService, TemplateFolderService>();
builder.Services.AddScoped<ITestSuiteTemplateService, TestSuiteTemplateService>();


// Add controllers for API endpoints
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();


// Add SignalR for real-time notifications
builder.Services.AddSignalR();
builder.Services.AddSingleton<Testify.Interfaces.IUserPresenceService, Testify.Services.UserPresenceService>();

var app = builder.Build();

// Seed users and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
        await UserSeeder.SeedUsersAsync(services);
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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// Map API controllers
app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<CallHub>("/hubs/call");

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











