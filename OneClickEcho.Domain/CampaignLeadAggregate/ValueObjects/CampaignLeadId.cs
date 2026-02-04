using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;

public sealed class CampaignLeadId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private CampaignLeadId(Guid value)
    {
        Value = value;
    }

    public static new CampaignLeadId Create(Guid value)
    {
        return new CampaignLeadId(value);
    }

    public static new CampaignLeadId CreateUnique()
    {
        return new CampaignLeadId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}