using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.ApplicationUserAggregate.ValueObjects;

public sealed class ApplicationUserCompanyId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private ApplicationUserCompanyId(Guid value)
    {
        Value = value;
    }

    public static new ApplicationUserCompanyId Create(Guid value)
    {
        return new ApplicationUserCompanyId(value);
    }

    public static new ApplicationUserCompanyId CreateUnique()
    {
        return new ApplicationUserCompanyId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}