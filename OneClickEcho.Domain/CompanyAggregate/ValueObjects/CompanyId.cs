using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CompanyAggregate.ValueObjects;

public sealed class CompanyId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private CompanyId(Guid value)
    {
        Value = value;
    }

    public static new CompanyId Create(Guid value)
    {
        return new CompanyId(value);
    }

    public static new CompanyId CreateUnique()
    {
        return new CompanyId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}