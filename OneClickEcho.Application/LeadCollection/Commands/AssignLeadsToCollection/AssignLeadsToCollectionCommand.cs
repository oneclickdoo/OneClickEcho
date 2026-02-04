using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Commands.AssignLeadsToCollection;

public sealed record AssignLeadsToCollectionCommand(
    Guid LeadCollectionId,
    List<SingleLeadDto> Leads
) : ICommand<AssignLeadsToCollectionResponse>;

public record SingleLeadDto(Guid LeadId);