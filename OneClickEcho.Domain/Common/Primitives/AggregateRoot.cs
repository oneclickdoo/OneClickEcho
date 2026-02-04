namespace OneClickEcho.Domain.Common.Primitives;

public abstract class AggregateRoot<TId> : AggregateRootGenericType<TId, Guid> where TId : AggregateRootId<Guid>
{
    public new TId Id { get; private set; }
    protected AggregateRoot(TId id) : base(id)
    {
        Id = id;
    }
}