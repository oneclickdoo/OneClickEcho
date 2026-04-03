using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Commands.DeleteLeadCollection;

public sealed record DeleteLeadCollectionCommand(Guid LeadCollectionId) : ICommand<DeleteLeadCollectionResponse>;