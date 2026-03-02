using Testify.Shared.Interfaces;

namespace Testify.Middleware;

public class MaintenanceMiddleware(RequestDelegate next)
{
    // Paths that always bypass maintenance mode
    private static readonly string[] BypassPrefixes =
    [
        "/maintenance",
        "/account/login",
        "/account/logout",
        "/account/forgotpassword",
        "/_framework",
        "/_blazor",
        "/_content",
        "/api/admin",
        "/admin",
        "/hubs",
    ];

    public async Task InvokeAsync(HttpContext context, ISystemSettingsService settings)
    {
        var isMaintenanceMode = await settings.GetBoolSettingAsync("MaintenanceMode", false);

        if (isMaintenanceMode)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            var isBypassed = BypassPrefixes.Any(prefix => path.StartsWith(prefix))
                             || path.StartsWith("/_")
                             || IsStaticFile(path);

            if (!isBypassed)
            {
                // Admins always get through
                if (context.User.Identity?.IsAuthenticated == true
                    && context.User.IsInRole("Admin"))
                {
                    await next(context);
                    return;
                }

                context.Response.Redirect("/maintenance");
                return;
            }
        }

        await next(context);
    }

    private static bool IsStaticFile(string path) =>
        path.EndsWith(".js")  || path.EndsWith(".css") || path.EndsWith(".png")  ||
        path.EndsWith(".jpg") || path.EndsWith(".svg") || path.EndsWith(".ico")  ||
        path.EndsWith(".woff")|| path.EndsWith(".woff2")|| path.EndsWith(".ttf") ||
        path.EndsWith(".map") || path.EndsWith(".json") || path.EndsWith(".gz");
}
