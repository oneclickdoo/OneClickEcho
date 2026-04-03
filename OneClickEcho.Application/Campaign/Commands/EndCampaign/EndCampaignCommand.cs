using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.EndCampaign
{
    public sealed record EndCampaignCommand(Guid CampaignId) : ICommand<EndCampaignResponse>;
}
