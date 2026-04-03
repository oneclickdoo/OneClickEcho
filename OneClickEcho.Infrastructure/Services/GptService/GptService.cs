using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.GptRequestAggregate.Repositories;

namespace OneClickEcho.Infrastructure.Services.GptService;

public class GptService(GptRequestStrategyFactory gptRequestStrategyFactory,
    IGptRequestRepository gptRequestRepository, IUnitOfWork unitOfWork) : IGptService
{
    private readonly GptRequestStrategyFactory _gptRequestStrategyFactory = gptRequestStrategyFactory;
    private readonly IGptRequestRepository _gptRequestRepository = gptRequestRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<GptRequest>> SendGptRequestAsync(GptRequestDto request, CancellationToken cancellationToken = default)
    {
        IGptRequestStrategy gptRequestStrategy = _gptRequestStrategyFactory.GetGptRequestStrategy(request.RequestType);

        Result<GptRequest> response = await gptRequestStrategy.SendGptRequestAsync(request, cancellationToken);

        _gptRequestRepository.Add(response.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}