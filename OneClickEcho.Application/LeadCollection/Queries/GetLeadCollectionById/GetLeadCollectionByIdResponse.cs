namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionById;

public sealed record GetLeadCollectionByIdResponse(
    Guid LeadCollectionId,
    string CollectionName,
    DateTime CreatedAt
);
