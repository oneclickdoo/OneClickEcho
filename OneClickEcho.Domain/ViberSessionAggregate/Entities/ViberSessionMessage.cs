using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.ViberSessionAggregate.Enums;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

namespace OneClickEcho.Domain.ViberSessionAggregate.Entities;

public sealed class ViberSessionMessage : Entity<ViberSessionMessageId>
{
    public ViberSessionMessage(
        ViberSessionId sessionId,
        ViberSessionDirection direction,
        int comtradeMessageType,
        string? body,
        string? media,
        string? surveyOptionsJson,
        bool billedAsPromotion) : base(ViberSessionMessageId.CreateUnique())
    {
        SessionId = sessionId;
        Direction = direction;
        ComtradeMessageType = comtradeMessageType;
        Body = body;
        Media = media;
        SurveyOptionsJson = surveyOptionsJson;
        BilledAsPromotion = billedAsPromotion;
    }

    public ViberSessionId SessionId { get; set; } = default!;

    public ViberSessionDirection Direction { get; set; }

    public int ComtradeMessageType { get; set; }

    public string? Body { get; set; }

    public string? Media { get; set; }

    public string? SurveyOptionsJson { get; set; }

    public long ViberMessageId { get; set; }

    public string? MessageToken { get; set; }

    public bool IsSent { get; set; }

    public bool BilledAsPromotion { get; set; }

    public string? SendError { get; set; }

    public ViberSessionMessage() : base(ViberSessionMessageId.CreateUnique())
    {
    }
}
