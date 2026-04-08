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
    string Message,
    
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
    int? ViberVideoDuration
    ) : ICommand<SendApiMessageResponse>;