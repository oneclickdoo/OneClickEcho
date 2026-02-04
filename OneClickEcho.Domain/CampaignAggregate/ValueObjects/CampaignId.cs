using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignAggregate.ValueObjects;

public sealed class CampaignId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private CampaignId(Guid value)
    {
        Value = value;
    }

    public static new CampaignId Create(Guid value)
    {
        return new CampaignId(value);
    }

    public static new CampaignId CreateUnique()
    {
        return new CampaignId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}