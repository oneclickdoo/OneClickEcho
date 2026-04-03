using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadCollections;

public class GetCampaignLeadCollectionsResponse(IPagedList<Domain.LeadCollectionAggregate.LeadCollection> items)
    : PagedListDto<Domain.LeadCollectionAggregate.LeadCollection, GetCampaignLeadCollectionDto>(items)
{
    public override List<GetCampaignLeadCollectionDto> ConvertTToTDto(List<Domain.LeadCollectionAggregate.LeadCollection> items)
    {
        List<GetCampaignLeadCollectionDto> result = [];

        foreach (Domain.LeadCollectionAggregate.LeadCollection leadCollection in items)
        {
            result.Add(new GetCampaignLeadCollectionDto(
                leadCollection.Id.Value,
                leadCollection.CollectionName,
                leadCollection.CreatedAt
            ));
        }

        return result;
    }
}

public record GetCampaignLeadCollectionDto(
    Guid LeadCollectionId,
    string CollectionName,
    DateTime CreatedAt
    );