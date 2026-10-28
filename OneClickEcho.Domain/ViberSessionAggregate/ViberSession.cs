using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

namespace OneClickEcho.Domain.ViberSessionAggregate;

public sealed class ViberSession : AggregateRoot<ViberSessionId>
{
    public const int MaxOutbound = 60;
    public const int MaxConsecutive = 10;
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    public ViberSession(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        DateTime startedAt) : base(ViberSessionId.CreateUnique())
    {
        CompanyId = companyId;
        Sender = sender;
        PhoneNumber = phoneNumber;
        StartedAt = startedAt;
        ExpiresAt = startedAt;
    }

    public CompanyId CompanyId { get; set; } = default!;

    public string Sender { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? ComtradeSessionId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastInboundAt { get; set; }

    public int TotalOutbound { get; set; }

    public int ConsecutiveOutbound { get; set; }

    public bool IsOpen(DateTime utcNow) =>
        LastInboundAt is not null && ExpiresAt > utcNow;

    public void OpenFromInbound(DateTime receivedUtc)
    {
        StartedAt = receivedUtc;
        ExpiresAt = receivedUtc.Add(Window);
        LastInboundAt = receivedUtc;
        ConsecutiveOutbound = 0;
        TotalOutbound = 0;
    }

    public ViberSession() : base(ViberSessionId.CreateUnique())
    {
    }
}
