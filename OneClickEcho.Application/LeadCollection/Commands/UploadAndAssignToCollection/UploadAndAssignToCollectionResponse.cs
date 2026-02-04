namespace OneClickEcho.Application.LeadCollection.Commands.UploadAndAssignToCollection;

public sealed record UploadAndAssignLeadsToCollectionResponse(
    Guid Id,
    int LeadsAdded);