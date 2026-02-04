using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Common.Services.GptService;

public interface IGptService
{
    Task<Result<Domain.GptRequestAggregate.GptRequest>> SendGptRequestAsync(GptRequestDto request, CancellationToken cancellationToken = default);
}