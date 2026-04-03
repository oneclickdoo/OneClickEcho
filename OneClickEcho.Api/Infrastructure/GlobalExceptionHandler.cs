using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OneClickEcho.Application.Exceptions;

namespace OneClickEcho.App.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

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
            problemDetails = new ProblemDetails
            {
                Title = "public Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await context.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }
}