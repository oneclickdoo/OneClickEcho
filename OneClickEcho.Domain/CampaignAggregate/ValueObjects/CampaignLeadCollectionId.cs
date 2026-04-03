using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignAggregate.ValueObjects;

public sealed class CampaignLeadCollectionId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private CampaignLeadCollectionId(Guid value)
    {
        Value = value;
    }

    public static new CampaignLeadCollectionId Create(Guid value)
    {
        return new CampaignLeadCollectionId(value);
    }

    public static new CampaignLeadCollectionId CreateUnique()
    {
        return new CampaignLeadCollectionId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}