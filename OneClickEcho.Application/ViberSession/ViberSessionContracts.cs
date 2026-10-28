using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.ViberSessionAggregate.Enums;

namespace OneClickEcho.Application.ViberSession;

public record ViberSessionMessageResponse(
    Guid Id,
    string Direction,
    int ComtradeMessageType,
    string? Body,
    string? Media,
    string? SurveyOptionsJson,
    long ViberMessageId,
    bool IsSent,
    bool BilledAsPromotion,
    string? SendError,
    DateTime CreatedAt);

public record ViberSessionResponse(
    Guid Id,
    Guid CompanyId,
    string Sender,
    string PhoneNumber,
    string? ComtradeSessionId,
    DateTime StartedAt,
    DateTime ExpiresAt,
    DateTime? LastInboundAt,
    int TotalOutbound,
    int ConsecutiveOutbound,
    bool IsOpen,
    IReadOnlyList<ViberSessionMessageResponse> Messages);

public record ListViberSessionsQuery(Guid CompanyId) : IQuery<List<ViberSessionResponse>>;

public record GetViberSessionQuery(Guid CompanyId, Guid SessionId) : IQuery<ViberSessionResponse>;

public record GetViberSessionByPhoneQuery(Guid CompanyId, string Sender, string PhoneNumber) : IQuery<ViberSessionResponse>;

public record GetViberSessionByPhoneApiQuery(Guid CompanyId, string ApiPassword, string Sender, string PhoneNumber)
    : IQuery<ViberSessionResponse>;

public record SendViberSessionCommand(
    Guid CompanyId,
    string Sender,
    string PhoneNumber,
    ViberSessionContentKind Kind,
    string? Text,
    string? Media,
    string? QuickRepliesJson,
    short? SurveyUiType,
    bool AllowOutsideSession) : ICommand<ViberSessionResponse>;

public record SendViberSessionApiCommand(
    Guid CompanyId,
    string ApiPassword,
    string Sender,
    string PhoneNumber,
    ViberSessionContentKind Kind,
    string? Text,
    string? Media,
    string? QuickRepliesJson,
    short? SurveyUiType,
    bool AllowOutsideSession) : ICommand<ViberSessionResponse>;
