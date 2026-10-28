using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;

namespace OneClickEcho.Application.Campaign.Commands.EditCampaign;

public record EditCampaignCommand(
    Guid CampaignId,
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
    int? ViberFileSize,
    int? ViberVideoDuration,
    int? ViberValidity,
    CampaignViberContentKind ViberContentKind,
    string? ViberSurveyOptionsJson,
    string? ViberImageRatio,
    short? ViberSurveyUiType,
    string? ViberCarouselJson,
    Guid? ViberTemplateId,
    string? ViberTemplateParamsJson,
    bool IsSms,
    string? SmsSender,
    string? SmsMessage,
    string? TestPhoneNumber,
    CampaignSendingType SendingType,
    DateTime? SendingDatetime
) : ICommand<EditCampaignResponse>;