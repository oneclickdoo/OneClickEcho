using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;

public sealed class ReceivedMessageId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private ReceivedMessageId(Guid value)
    {
        Value = value;
    }

    public static new ReceivedMessageId Create(Guid value)
    {
        return new ReceivedMessageId(value);
    }

    public static new ReceivedMessageId CreateUnique()
    {
        return new ReceivedMessageId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}