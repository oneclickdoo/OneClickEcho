using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;

public class GetLeadCollectionsHandler(ILeadCollectionRepository leadCollectionRepository) : IQueryHandler<GetLeadCollectionsQuery, GetLeadCollectionsResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

    public async Task<Result<GetLeadCollectionsResponse>> Handle(GetLeadCollectionsQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Common.Queries.IPagedList<Domain.LeadCollectionAggregate.LeadCollection> leadCollections =
            await _leadCollectionRepository.GetAllAsync(query, query.SearchString, cancellationToken);

        return new GetLeadCollectionsResponse(leadCollections);
    }
}