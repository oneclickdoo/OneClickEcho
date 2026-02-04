using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Queries.ExportCampaignLeads
{
    public record ExportCampaignLeadsQuery(
        Guid CampaignId,
        CampaignLeadSMSStatus? SmsStatus = null,
        CampaignLeadViberStatus? ViberStatus = null
    ) : IQuery<ExportCampaignLeadsResponse>;
}
