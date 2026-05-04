using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;

public sealed class ViberDeliveryEventId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private ViberDeliveryEventId(Guid value)
    {
        Value = value;
    }

    public static new ViberDeliveryEventId Create(Guid value)
    {
        return new ViberDeliveryEventId(value);
    }

    public static new ViberDeliveryEventId CreateUnique()
    {
        return new ViberDeliveryEventId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
