using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;

namespace OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;

public sealed record SendApiMessageCommand(
    // Auth
    Guid CompanyId,
    string ApiPassword,
    string Sender,
    ApiMessageType ApiMessageType,
    
    // Core
    string PhoneNumber,
    string? Message,
    
    // Viber — media URL must be reachable by Comtrade/Viber over the public internet (HTTPS), or a file name served under your configured public uploads base URL.
    string? ViberMedia,
    string? ViberButtonUrl,
    string? ViberButtonUrlTitle,
    int? ViberValidity,
    
    // Extra
    bool? HasSmsFallback,
    
    // Sms
    string? SmsSender,
    string? SmsMessage,

    /// <summary>Optional. Upload file name or public HTTPS URL; Comtrade fetches the thumbnail over the internet.</summary>
    string? ViberVideoThumbnail,
    int? ViberFileSize,
    int? ViberVideoDuration,

    /// <summary>Optional Comtrade ImageFormat: gif or img. Gif is also inferred from a .gif media URL.</summary>
    string? ViberImageFormat,
    /// <summary>square, portrait, or landscape. Used for image messages.</summary>
    string? ViberImageRatio,
    /// <summary>JSON array of 2–10 survey options. Sends Comtrade type 801.</summary>
    string? ViberSurveyOptionsJson,
    /// <summary>1 radio, 2 buttons.</summary>
    short? ViberSurveyUiType,
    /// <summary>Carousel JSON (2–5 cards). Sends Comtrade type 901.</summary>
    string? ViberCarouselJson,
    /// <summary>Local id of an approved transactional template. Sends Comtrade type 1702.</summary>
    Guid? ViberTemplateId,
    /// <summary>JSON object of template variable values.</summary>
    string? ViberTemplateParamsJson
    ) : ICommand<SendApiMessageResponse>;