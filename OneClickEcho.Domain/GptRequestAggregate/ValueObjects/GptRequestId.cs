using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.GptRequestAggregate.ValueObjects;

public sealed class GptRequestId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private GptRequestId(Guid value)
    {
        Value = value;
    }

    public static new GptRequestId Create(Guid value)
    {
        return new GptRequestId(value);
    }

    public static new GptRequestId CreateUnique()
    {
        return new GptRequestId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}