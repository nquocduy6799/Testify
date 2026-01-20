using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Testify.Client.Features.Kanban.Services;
using Testify.Client.Features.Projects.Services;
using Testify.Client.Interfaces;

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

await builder.Build().RunAsync();
