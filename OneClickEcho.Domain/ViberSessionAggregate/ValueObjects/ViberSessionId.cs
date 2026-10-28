using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

public sealed class ViberSessionId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ViberSessionId(Guid value)
    {
        Value = value;
    }

    public static new ViberSessionId Create(Guid value) => new(value);

    public static new ViberSessionId CreateUnique() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
