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
    
    // Viber
    string? ViberMedia,
    string? ViberButtonUrl,
    string? ViberButtonUrlTitle,
    int? ViberValidity,
    
    // Extra
    bool? HasSmsFallback,
    
    // Sms
    string? SmsSender,
    string? SmsMessage
    ) : ICommand<SendApiMessageResponse>;