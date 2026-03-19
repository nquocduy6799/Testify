using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Testify.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleApiExceptionAsync(context, ex);
        }
    }

    private async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = exception switch
        {
            KeyNotFoundException    => (StatusCodes.Status404NotFound,            "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,       "Forbidden"),
            ArgumentNullException
            or ArgumentException    => (StatusCodes.Status400BadRequest,          "Bad request"),
            _                       => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = environment.IsDevelopment() ? exception.ToString() : null,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
