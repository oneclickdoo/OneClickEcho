using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionById;

public sealed record GetLeadCollectionByIdQuery(Guid LeadCollectionId) : IQuery<GetLeadCollectionByIdResponse>;