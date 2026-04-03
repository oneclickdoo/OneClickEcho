using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.AddLeadsToCollectionFromStatus;

public record AddLeadsToCollectionFromStatusCommand(
    Guid LeadCollectionId,
    Guid CampaignId,
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus
) : ICommand<AddLeadsToCollectionFromStatusResponse>;

public record AddLeadsToCollectionFromStatusCommandDto(
    Guid LeadCollectionId,
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus
);
