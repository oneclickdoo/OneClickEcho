using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;

public class GetLeadCollectionsResponse(IPagedList<Domain.LeadCollectionAggregate.LeadCollection> items)
    : PagedListDto<Domain.LeadCollectionAggregate.LeadCollection, GetLeadCollectionDto>(items)
{
    public override List<GetLeadCollectionDto> ConvertTToTDto(List<Domain.LeadCollectionAggregate.LeadCollection> items)
    {
        List<GetLeadCollectionDto> result = [];

        foreach (Domain.LeadCollectionAggregate.LeadCollection leadCollection in items)
        {
            result.Add(new GetLeadCollectionDto(
                LeadCollectionId: leadCollection.Id.Value,
                CollectionName: leadCollection.CollectionName,
                CreatedAt: leadCollection.CreatedAt
            ));
        }

        return result;
    }
}

public record GetLeadCollectionDto(
    Guid LeadCollectionId,
    string CollectionName,
    DateTime CreatedAt
);