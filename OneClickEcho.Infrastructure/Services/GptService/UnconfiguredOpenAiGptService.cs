using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate;

namespace OneClickEcho.Infrastructure.Services.GptService;

/// <summary>
/// Used when OpenAi:ApiKey or OpenAi:Model is missing so the host can start; GPT endpoints return a clear failure.
/// </summary>
public sealed class UnconfiguredOpenAiGptService : IGptService
{
    private static readonly Error NotConfigured = new(
        "OpenAi.NotConfigured",
        "OpenAI is not configured. Set OpenAi:ApiKey and OpenAi:Model (e.g. user-secrets or env). If you use a Blackbird or other proxy, set OpenAi:BaseUrl as well.");

    public Task<Result<GptRequest>> SendGptRequestAsync(GptRequestDto request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<GptRequest>(NotConfigured));
    }
}
