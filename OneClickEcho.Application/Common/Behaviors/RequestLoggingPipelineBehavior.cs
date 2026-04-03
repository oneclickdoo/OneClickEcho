using MediatR;
using Microsoft.Extensions.Logging;
using OneClickEcho.Domain.Common.Shared;
using Serilog.Context;

namespace OneClickEcho.Application.Common.Behaviors;

public class RequestLoggingPipelineBehavior<TRequest, TResponse>(ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>>
    logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class where TResponse : Result
{
    private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _logger.LogInformation($"Processing request: {requestName}");

        TResponse result = await next();

        if (result.IsSuccess)
        {
            _logger.LogInformation($"Successfully processed request: {requestName}");
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
            {
                _logger.LogError($"Failed to process request: {requestName}");
            }
        }

        return result;
    }
}