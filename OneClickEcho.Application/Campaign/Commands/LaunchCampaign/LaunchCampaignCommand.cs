using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.LaunchCampaign
{
    public sealed record LaunchCampaignCommand(Guid CampaignId) : ICommand<LaunchCampaignResponse>;
}
