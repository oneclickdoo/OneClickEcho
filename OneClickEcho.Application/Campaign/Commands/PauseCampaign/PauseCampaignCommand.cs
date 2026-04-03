using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.PauseCampaign
{
    public sealed record PauseCampaignCommand(Guid CampaignId) : ICommand<PauseCampaignResponse>;
}
