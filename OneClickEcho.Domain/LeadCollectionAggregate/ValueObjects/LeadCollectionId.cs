using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

public sealed class LeadCollectionId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private LeadCollectionId(Guid value)
    {
        Value = value;
    }

    public static new LeadCollectionId Create(Guid value)
    {
        return new LeadCollectionId(value);
    }

    public static new LeadCollectionId CreateUnique()
    {
        return new LeadCollectionId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}