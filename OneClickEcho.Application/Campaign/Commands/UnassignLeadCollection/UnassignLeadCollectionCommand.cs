using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Commands.UnassignLeadCollection;

public record UnassignLeadCollectionCommand(
    Guid CampaignId,
    Guid LeadCollectionId
    ) : ICommand<UnassignLeadCollectionResponse>;
