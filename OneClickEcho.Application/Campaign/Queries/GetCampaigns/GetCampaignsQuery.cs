using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaigns;

public class GetCampaignsQuery : BasePagedQuery, IQuery<GetCampaignsResponse>
{
    public Guid CompanyId;
}