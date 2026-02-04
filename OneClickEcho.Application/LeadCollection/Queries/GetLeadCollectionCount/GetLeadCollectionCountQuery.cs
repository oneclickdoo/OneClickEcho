using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionCount;

public sealed record GetLeadCollectionCountQuery(List<Guid> LeadCollectionIds) : IQuery<GetLeadCollectionCountResponse>;