using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.LeadCollection.Queries.SearchLeadCollections;

public class SearchLeadCollectionsHandler(ILeadCollectionRepository leadCollectionRepository)
    : IQueryHandler<SearchLeadCollectionsQuery, List<GetLeadCollectionDto>>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

    public async Task<Result<List<GetLeadCollectionDto>>> Handle(SearchLeadCollectionsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.LeadCollectionAggregate.LeadCollection> leadCollections = await _leadCollectionRepository
            .SearchByNameAsync(request.Name is not null ? request.Name : "", CampaignId.Create(request.CampaignId), cancellationToken);

        List<GetLeadCollectionDto> leadCollectionDtos = leadCollections
            .Select(x => new GetLeadCollectionDto(x.Id.Value, x.CollectionName, x.CreatedAt))
            .ToList();

        return leadCollectionDtos;
    }
}