using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadCollections;

public class GetCampaignLeadCollectionsHandler(ICampaignRepository campaignRepository,
    ILeadCollectionRepository leadCollectionRepository)
    : IQueryHandler<GetCampaignLeadCollectionsQuery, GetCampaignLeadCollectionsResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

    public async Task<Result<GetCampaignLeadCollectionsResponse>> Handle(GetCampaignLeadCollectionsQuery query, CancellationToken cancellationToken)
    {
        // get campaign
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<GetCampaignLeadCollectionsResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{query.CampaignId}\" does not exist."
            ));
        }

        Domain.Common.Queries.IPagedList<Domain.LeadCollectionAggregate.LeadCollection> leadCollections =
            await _leadCollectionRepository.GetAllByCampaignIdAsync(campaign.Id, query, cancellationToken);

        return new GetCampaignLeadCollectionsResponse(leadCollections);
    }
}