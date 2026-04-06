using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate.Enums;
using OneClickEcho.Domain.NounCaseAggregate.Repositories;

namespace OneClickEcho.Application.GptRequest.Commands.ConvertNounCases;

public class ConvertNounCasesHandler(IGptService gptService, INounCaseRepository nounCaseRepository)
    : ICommandHandler<ConvertNounCasesCommand, ConvertNounCasesResponse>
{
    private readonly IGptService _gptService = gptService;
    private readonly INounCaseRepository _nounCaseRepository = nounCaseRepository;

    public async Task<Result<ConvertNounCasesResponse>> Handle(ConvertNounCasesCommand request, CancellationToken cancellationToken)
    {
        Domain.NounCaseAggregate.NounCase? prev = await _nounCaseRepository.GetByNominativeAsync(request.Name, cancellationToken);

        if (prev is not null)
        {
            return new ConvertNounCasesResponse(prev.Vocative!);
        }

        Result<Domain.GptRequestAggregate.GptRequest> response = await _gptService.SendGptRequestAsync(new GptRequestDto
        {
            RequestMessage = request.Name,
            RequestType = GptRequestType.ConvertNounCases,
        }, cancellationToken);

        if (response.IsFailure)
        {
            return Result.Failure<ConvertNounCasesResponse>(response.Error);
        }

        return new ConvertNounCasesResponse(response.Value.ResponseMessage);
    }
}