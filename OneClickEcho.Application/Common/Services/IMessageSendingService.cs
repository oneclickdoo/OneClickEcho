using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate;

namespace OneClickEcho.Application.Common.Services;

public interface IMessageSendingService
{
    /// <param name="viberOnlyForProvidedLeads">If true, only the Viber send runs for <paramref name="leads"/>; SMS is skipped. Use for retries.</param>
    public Task SendMessagesForCampaignId(CampaignId campaignId, List<Domain.LeadAggregate.Lead>? leads = null,
        bool viberOnlyForProvidedLeads = false);

    public Task SendApiMessages(CompanyId companyId, List<Domain.ApiMessageAggregate.ApiMessage> apiMessages, ApiMessageType messageType);

    public Task SendTestMessages(Domain.CampaignAggregate.Campaign campaign, TestMessage testMessage);
}