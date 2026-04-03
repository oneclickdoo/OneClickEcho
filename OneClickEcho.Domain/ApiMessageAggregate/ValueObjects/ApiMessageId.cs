using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;

public sealed class ApiMessageId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ApiMessageId(Guid value)
    {
        Value = value;
    }

    public static new ApiMessageId Create(Guid value)
    {
        return new ApiMessageId(value);
    }

    public static new ApiMessageId CreateUnique()
    {
        return new ApiMessageId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}