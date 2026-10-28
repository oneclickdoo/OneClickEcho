using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using SessionEntity = OneClickEcho.Domain.ViberSessionAggregate.ViberSession;

namespace OneClickEcho.Application.ViberSession;

public static class ViberSessionMapper
{
    public static ViberSessionResponse Map(SessionEntity session, IReadOnlyList<ViberSessionMessage> messages, DateTime utcNow) =>
        new(
            session.Id.Value,
            session.CompanyId.Value,
            session.Sender,
            session.PhoneNumber,
            session.ComtradeSessionId,
            session.StartedAt,
            session.ExpiresAt,
            session.LastInboundAt,
            session.TotalOutbound,
            session.ConsecutiveOutbound,
            session.IsOpen(utcNow),
            messages.Select(m => new ViberSessionMessageResponse(
                m.Id.Value,
                m.Direction.ToString(),
                m.ComtradeMessageType,
                m.Body,
                m.Media,
                m.SurveyOptionsJson,
                m.ViberMessageId,
                m.IsSent,
                m.BilledAsPromotion,
                m.SendError,
                m.CreatedAt)).ToList());
}
