using System.Net.Http;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate;

namespace OneClickEcho.Infrastructure.Services.GptService;

internal static class OpenAiConnectionErrors
{
    public static Result<GptRequest> AsFailure(Exception ex, Uri? baseAddress)
    {
        string host = baseAddress?.GetLeftPart(UriPartial.Authority) ?? "OpenAI endpoint";

        string message = ex switch
        {
            HttpRequestException h => $"Could not reach {host}. {h.Message}",
            TaskCanceledException when ex.InnerException is TimeoutException =>
                $"Request to {host} timed out.",
            TaskCanceledException =>
                $"Request to {host} was cancelled or timed out.",
            IOException io => $"Network I/O error calling {host}. {io.Message}",
            _ => $"Unexpected error calling {host}. {ex.Message}"
        };

        return Result.Failure<GptRequest>(new Error("GptRequest.ConnectionFailed", message));
    }
}
