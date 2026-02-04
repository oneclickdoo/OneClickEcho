using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;

namespace OneClickEcho.Application.LeadCollection.Queries.SearchLeadCollections;

public sealed record SearchLeadCollectionsQuery(
    Guid CampaignId,
    string? Name
    ) : IQuery<List<GetLeadCollectionDto>>;
