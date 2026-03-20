using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException      => (StatusCodes.Status404NotFound,            "Not Found"),
            ValidationException    => (StatusCodes.Status400BadRequest,          "Bad Request"),
            ForbiddenException     => (StatusCodes.Status403Forbidden,           "Forbidden"),
            ConflictException      => (StatusCodes.Status409Conflict,            "Conflict"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,  "Unauthorized"),
            _                      => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "Handled exception ({Status}): {Message}", statusCode, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
