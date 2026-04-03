using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate.Enums;

namespace OneClickEcho.Application.GptRequest.Commands.EnhanceCampaignMessage;

public class EnhanceCampaignMessageHandler(IGptService gptService)
    : ICommandHandler<EnhanceCampaignMessageCommand, EnhanceCampaignMessageResponse>
{
    private readonly IGptService _gptService = gptService;

    public async Task<Result<EnhanceCampaignMessageResponse>> Handle(EnhanceCampaignMessageCommand request, CancellationToken cancellationToken)
    {
        Result<Domain.GptRequestAggregate.GptRequest> response = await _gptService.SendGptRequestAsync(new GptRequestDto
        {
            RequestMessage = request.RequestMessage,
            RequestType = GptRequestType.EnhanceCampaignMessage,
            CampaignId = request.CampaignId
        }, cancellationToken);

        return new EnhanceCampaignMessageResponse(response.Value.Id.Value, response.Value.ResponseMessage);
    }
}