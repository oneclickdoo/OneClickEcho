using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.NounCaseAggregate.ValueObjects;

public sealed class NounCaseId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private NounCaseId(Guid value)
    {
        Value = value;
    }

    public static new NounCaseId Create(Guid value)
    {
        return new NounCaseId(value);
    }

    public static new NounCaseId CreateUnique()
    {
        return new NounCaseId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}