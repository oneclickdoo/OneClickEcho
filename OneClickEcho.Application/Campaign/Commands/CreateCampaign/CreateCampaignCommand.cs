using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.CreateCampaign;

public sealed record CreateCampaignCommand(
    string CampaignName,
    Guid CompanyId,
    bool IsViber,
    bool FallbackToSMS,
    bool IsViberReceivable,
    string? ViberSender,
    string? ViberMessage,
    string? ViberButtonUrl,
    string? ViberButtonUrlTitle,
    string? ViberVideoThumbnail,
    bool IsSms,
    string? SmsSender,
    string? SmsMessage,
    string? TestPhoneNumbers,
    CampaignSendingType SendingType,
    DateTime? SendingDatetime
    ) : ICommand<CreateCampaignResponse>;