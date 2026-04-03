using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.TestMessageAggregate.ValueObjects;

public sealed class TestMessageId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private TestMessageId(Guid value)
    {
        Value = value;
    }

    public static new TestMessageId Create(Guid value)
    {
        return new TestMessageId(value);
    }

    public static new TestMessageId CreateUnique()
    {
        return new TestMessageId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}