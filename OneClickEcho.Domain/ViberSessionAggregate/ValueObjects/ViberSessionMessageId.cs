using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

public sealed class ViberSessionMessageId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private ViberSessionMessageId(Guid value)
    {
        Value = value;
    }

    public static new ViberSessionMessageId Create(Guid value) => new(value);

    public static new ViberSessionMessageId CreateUnique() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
