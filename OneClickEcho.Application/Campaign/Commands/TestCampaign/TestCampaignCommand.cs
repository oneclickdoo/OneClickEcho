using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.TestCampaign
{
    public sealed record TestCampaignCommand(Guid CampaignId, string PhoneNumber) : ICommand<TestCampaignResponse>;
}
