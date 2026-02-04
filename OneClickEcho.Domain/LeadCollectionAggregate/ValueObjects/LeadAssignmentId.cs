using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

public sealed class LeadAssignmentId : EntityId<Guid>
{
    public override Guid Value { get; protected set; }

    private LeadAssignmentId(Guid value)
    {
        Value = value;
    }

    public static new LeadAssignmentId Create(Guid value)
    {
        return new LeadAssignmentId(value);
    }

    public static new LeadAssignmentId CreateUnique()
    {
        return new LeadAssignmentId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}