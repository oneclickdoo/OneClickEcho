using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeads
{
    public class GetCampaignLeadsQuery : BasePagedQuery, IQuery<GetCampaignLeadsResponse>
    {
        public Guid CampaignId;
    }
}
