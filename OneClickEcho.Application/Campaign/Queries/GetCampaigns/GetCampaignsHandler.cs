using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaigns;

public class GetCampaignsHandler(ICampaignRepository campaignRepository)
    : IQueryHandler<GetCampaignsQuery, GetCampaignsResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;

    public async Task<Result<GetCampaignsResponse>> Handle(GetCampaignsQuery query,
        CancellationToken cancellationToken)
    {
        IPagedList<Domain.CampaignAggregate.Campaign> campaigns = await _campaignRepository
            .GetAllAsync(query, cancellationToken);

        return new GetCampaignsResponse(campaigns);
    }
}