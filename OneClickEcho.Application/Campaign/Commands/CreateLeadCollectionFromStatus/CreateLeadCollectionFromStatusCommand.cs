using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.CreateLeadCollectionFromStatus;

public record CreateLeadCollectionFromStatusCommand(
    Guid CompanyId,
    Guid CampaignId,
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus,
    string CollectionName
) : ICommand<CreateLeadCollectionFromStatusResponse>;

public record CreateLeadCollectionFromStatusCommandDto(
    Guid CompanyId,
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus,
    string CollectionName
);
