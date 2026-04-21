using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.Application.Exceptions;

namespace OneClickEcho.App.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _env = env;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Exception occured: {Message}", exception.Message);

        ProblemDetails problemDetails;

        if (exception.GetType() == typeof(UnauthorizedException))
        {
            problemDetails = new ProblemDetails
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Unauthorized.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            string detail = _env.IsDevelopment()
                ? exception.Message
                : PublicErrorDetail(exception);

            problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await context.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }

    /// <summary>
    /// Avoid leaking stack traces in production while still surfacing timeouts / DB issues (e.g. Npgsql command timeout ~30s).
    /// </summary>
    private static string PublicErrorDetail(Exception exception)
    {
        for (Exception? e = exception; e != null; e = e.InnerException)
        {
            if (e is TimeoutException)
            {
                return "The database operation timed out. Please try again or narrow filters.";
            }

            string msg = e.Message;
            if (msg.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("timed out", StringComparison.OrdinalIgnoreCase))
            {
                return "The database operation timed out. Please try again or narrow filters.";
            }

            if (msg.Contains("Operation cancelled", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("operation was canceled", StringComparison.OrdinalIgnoreCase))
            {
                return "The database operation was cancelled or timed out. Please try again.";
            }
        }

        string fullName = exception.GetType().FullName ?? string.Empty;
        if (fullName.StartsWith("Npgsql.", StringComparison.Ordinal))
        {
            return "A database error occurred while processing the request.";
        }

        return "An unexpected error occurred.";
    }
}