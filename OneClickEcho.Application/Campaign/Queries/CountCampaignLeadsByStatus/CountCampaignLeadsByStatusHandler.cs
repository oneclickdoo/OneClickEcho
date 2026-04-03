using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.CountCampaignLeadsByStatus;

public class CountCampaignLeadsByStatusHandler(ICampaignRepository campaignRepository,
    ICampaignLeadRepository campaignLeadRepository)
    : IQueryHandler<CountCampaignLeadsByStatusQuery, CountCampaignLeadsByStatusResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;

    public async Task<Result<CountCampaignLeadsByStatusResponse>> Handle(CountCampaignLeadsByStatusQuery query, CancellationToken cancellationToken)
    {
        // get campaign
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<CountCampaignLeadsByStatusResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{query.CampaignId}\" does not exist."
            ));
        }

        CountCampaignLeadsByStatusDto result = await _campaignLeadRepository
            .CountCampaignLeadsByStatus(campaign, cancellationToken);

        return new CountCampaignLeadsByStatusResponse(result);
    }
}