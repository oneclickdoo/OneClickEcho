using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;

public sealed class ViberTemplateId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ViberTemplateId(Guid value)
    {
        Value = value;
    }

    public static new ViberTemplateId Create(Guid value) => new(value);

    public static new ViberTemplateId CreateUnique() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
