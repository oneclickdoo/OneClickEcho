using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateLeadCollection;

public sealed record CreateLeadCollectionCommand(
    string CollectionName,
    Guid CompanyId
) : ICommand<CreateLeadCollectionResponse>;