using OneClickEcho.Domain.CampaignAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignById;

public record GetCampaignByIdResponse(
    Guid CampaignId,
    Guid CompanyId,
    CampaignStatus Status,
    string Name,
    bool IsViber,
    bool FallbackToSMS,
    bool IsViberReceivable,
    string? ViberSender,
    string? ViberMessage,
    string? ViberMedia,
    string? ViberButtonUrl,
    string? ViberButtonUrlTitle,
    string? ViberVideoThumbnail,
    int? ViberValidity,
    bool IsSms,
    string? SmsSender,
    string? SmsMessage,
    string? TestPhoneNumber,
    CampaignSendingType SendingType,
    DateTime? SendingDatetime);