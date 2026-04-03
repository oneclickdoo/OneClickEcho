using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.AssignLeadCollection;

public record AssignLeadCollectionCommand(
    Guid CampaignId,
    Guid LeadCollectionId
    ) : ICommand<AssignLeadCollectionResponse>;

public record AssignLeadCollectionDto(Guid LeadCollectionId);
