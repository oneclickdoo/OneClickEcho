using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.CloneCampaign
{
    public record CloneCampaignCommand(Guid CampaignId) : ICommand<CloneCampaignResponse>;
}
