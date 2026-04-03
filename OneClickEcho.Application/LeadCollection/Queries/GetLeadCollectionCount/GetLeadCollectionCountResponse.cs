using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionCount;

public sealed record GetLeadCollectionCountResponse(
    List<LeadCollectionCountDto> Items
);
