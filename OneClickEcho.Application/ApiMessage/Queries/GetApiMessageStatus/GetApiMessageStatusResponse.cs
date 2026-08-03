namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessageStatus;

public sealed record GetApiMessageStatusResponse(
    Guid Id,
    bool IsSent,
    short ViberStatus,
    string? ViberStatusDescription,
    short SmsStatus,
    string? SmsStatusDescription,
    string PhoneNumber,
    long ViberMessageId,
    DateTime CreatedAt);
