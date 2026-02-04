using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionCount;

public class GetLeadCollectionCountHandler(ILeadCollectionRepository leadCollectionRepository)
    : IQueryHandler<GetLeadCollectionCountQuery, GetLeadCollectionCountResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

    public async Task<Result<GetLeadCollectionCountResponse>> Handle(GetLeadCollectionCountQuery query,
        CancellationToken cancellationToken)
    {
        var leadCollectionIds = query.LeadCollectionIds.Select(id => LeadCollectionId.Create(id)).ToArray();
        var counts = await _leadCollectionRepository.GetLeadCollectionCounts(leadCollectionIds, cancellationToken);

        return Result.Success(new GetLeadCollectionCountResponse(counts));
    }
}