using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate.Enums;

namespace OneClickEcho.Application.GptRequest.Commands.GenerateNewCampaignMessage;

public class GenerateNewCampaignMessageHandler(IGptService gptService)
    : ICommandHandler<GenerateNewCampaignMessageCommand, GenerateNewCampaignMessageResponse>
{
    private readonly IGptService _gptService = gptService;

    public async Task<Result<GenerateNewCampaignMessageResponse>> Handle(GenerateNewCampaignMessageCommand request, CancellationToken cancellationToken)
    {
        Result<Domain.GptRequestAggregate.GptRequest> response = await _gptService.SendGptRequestAsync(new GptRequestDto
        {
            RequestMessage = request.RequestMessage,
            RequestType = GptRequestType.GenerateNewCampaignMessage,
            CampaignId = request.CampaignId
        }, cancellationToken);

        return new GenerateNewCampaignMessageResponse(response.Value.Id.Value, response.Value.ResponseMessage);
    }
}