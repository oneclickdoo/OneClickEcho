using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.LeadAggregate.ValueObjects;

public sealed class LeadId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private LeadId(Guid value)
    {
        Value = value;
    }

    public static new LeadId Create(Guid value)
    {
        return new LeadId(value);
    }

    public static new LeadId CreateUnique()
    {
        return new LeadId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}