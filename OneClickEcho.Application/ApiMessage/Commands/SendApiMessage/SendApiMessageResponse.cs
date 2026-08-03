namespace OneClickEcho.Application.ApiMessage.Commands.SendApiMessage;

/// <summary>
/// Immediate ack after enqueue. Delivery fields update asynchronously — poll GET /api/Message/{id}/status.
/// </summary>
public sealed record SendApiMessageResponse(
    Guid Id,
    bool IsSent,
    short ViberStatus,
    string? ViberStatusDescription,
    short SmsStatus,
    string? SmsStatusDescription,
    string PhoneNumber,
    DateTime CreatedAt);
