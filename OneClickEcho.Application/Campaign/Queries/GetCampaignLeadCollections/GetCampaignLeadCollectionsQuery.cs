using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadCollections;

public class GetCampaignLeadCollectionsQuery : BasePagedQuery, IQuery<GetCampaignLeadCollectionsResponse>
{
    public Guid CampaignId;
}