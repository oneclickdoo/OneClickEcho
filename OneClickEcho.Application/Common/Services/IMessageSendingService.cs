using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate;

namespace OneClickEcho.Application.Common.Services;

public interface IMessageSendingService
{
    /// <param name="viberOnlyForProvidedLeads">If true, only the Viber send runs for <paramref name="leads"/>; SMS is skipped. Use for retries.</param>
    /// <param name="smsOnlyForProvidedLeads">If true, only the SMS send runs for <paramref name="leads"/>; Viber is skipped and no new SMS delivery schedule is created. Use for retries.</param>
    public Task SendMessagesForCampaignId(CampaignId campaignId, List<Domain.LeadAggregate.Lead>? leads = null,
        bool viberOnlyForProvidedLeads = false, bool smsOnlyForProvidedLeads = false);

    public Task SendApiMessages(CompanyId companyId, List<Domain.ApiMessageAggregate.ApiMessage> apiMessages, ApiMessageType messageType);

    public Task SendTestMessages(Domain.CampaignAggregate.Campaign campaign, TestMessage testMessage);
}