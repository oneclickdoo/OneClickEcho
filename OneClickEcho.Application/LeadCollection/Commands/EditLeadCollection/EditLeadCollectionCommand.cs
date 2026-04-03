using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Commands.EditLeadCollection;

public sealed record EditLeadCollectionCommand(
    Guid LeadCollectionId,
    string CollectionName
) : ICommand<EditLeadCollectionResponse>;