namespace OneClickEcho.Domain.Common.Primitives;

public abstract class AggregateRootId<TId> : ValueObject
{
    public abstract TId Value { get; protected set; }

    public static AggregateRootId<TId> Create(TId value)
    {
        throw new NotImplementedException();
    }

    public static AggregateRootId<TId> CreateUnique()
    {
        throw new NotImplementedException();
    }
}