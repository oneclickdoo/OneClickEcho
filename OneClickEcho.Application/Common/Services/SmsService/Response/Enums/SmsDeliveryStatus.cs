namespace OneClickEcho.Application.Common.Services.SmsService.Response.Enums;

public enum SmsDeliveryStatus
{
    // Error responses
    ErrorDescription = -200,
    ErrorInvalidReference = -201,
    ErrorInvalidUsernamePassword = -202,
    ErrorInvalidPhone = 3,
    ErrorSendingError = 6,

    // Reasonable states
    Pending = 5,
    Delivered = 1,
    Blacklisted = 8,
    Undelivered = 2,

    // Unknown
    UnknownStatus = -1
}