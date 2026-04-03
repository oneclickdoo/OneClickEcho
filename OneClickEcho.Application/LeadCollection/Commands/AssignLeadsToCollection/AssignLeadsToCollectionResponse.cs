namespace OneClickEcho.Application.LeadCollection.Commands.AssignLeadsToCollection;

public sealed record AssignLeadsToCollectionResponse(
    Guid Id,
    int LeadsAdded
);