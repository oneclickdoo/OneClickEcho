namespace OneClickEcho.Domain.Common.Primitives;

public abstract class EntityId<TId> : ValueObject
{
    public abstract TId Value { get; protected set; }

    public static EntityId<TId> Create(TId value)
    {
        throw new NotImplementedException();
    }

    public static ValueObject Create(object value)
    {
        return Create((TId)value);
    }

    public static EntityId<TId> CreateUnique()
    {
        throw new NotImplementedException();
    }
}