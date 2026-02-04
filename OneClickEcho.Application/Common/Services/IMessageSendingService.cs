using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate;

namespace OneClickEcho.Application.Common.Services;

public interface IMessageSendingService
{
    public Task SendMessagesForCampaignId(CampaignId campaignId, List<Domain.LeadAggregate.Lead>? leads = null);

    public Task SendApiMessages(CompanyId companyId, List<Domain.ApiMessageAggregate.ApiMessage> apiMessages, ApiMessageType messageType);

    public Task SendTestMessages(Domain.CampaignAggregate.Campaign campaign, TestMessage testMessage);
}