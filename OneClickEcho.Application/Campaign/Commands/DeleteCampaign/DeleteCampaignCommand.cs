using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.DeleteCampaign;

public record DeleteCampaignCommand(Guid CampaignId) : ICommand<DeleteCampaignResponse>;