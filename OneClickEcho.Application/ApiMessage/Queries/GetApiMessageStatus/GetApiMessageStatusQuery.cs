using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.ApiMessage.Queries.GetApiMessageStatus;

public sealed record GetApiMessageStatusQuery(
    Guid Id,
    Guid CompanyId,
    string ApiPassword
) : IQuery<GetApiMessageStatusResponse>;
