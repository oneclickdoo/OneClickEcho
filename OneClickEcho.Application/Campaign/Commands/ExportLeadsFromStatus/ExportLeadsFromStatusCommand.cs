using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.ExportLeadsFromStatus;

public record ExportLeadsFromStatusCommand(
    Guid CampaignId,
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus
) : ICommand<ExportLeadsFromStatusResponse>;

public record ExportLeadsFromStatusCommandDto(
    CampaignLeadViberStatus? ViberStatus,
    CampaignLeadSMSStatus? SmsStatus
);
