using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.GptRequest.Commands.GenerateNewCampaignMessage;

public sealed record GenerateNewCampaignMessageCommand(
    Guid CampaignId,
    string RequestMessage
) : ICommand<GenerateNewCampaignMessageResponse>;