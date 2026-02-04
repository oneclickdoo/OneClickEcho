using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.GptRequest.Commands.EnhanceCampaignMessage;

public sealed record EnhanceCampaignMessageCommand(
    Guid CampaignId,
    string RequestMessage
) : ICommand<EnhanceCampaignMessageResponse>;