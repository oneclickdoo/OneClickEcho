namespace OneClickEcho.Domain.Common.Primitives;

public abstract class AggregateRootGenericType<TId, TIdType> : Entity<TId> where TId : AggregateRootId<TIdType>
{
    public new AggregateRootId<TIdType> Id { get; private set; }
    protected AggregateRootGenericType(TId id) : base(id)
    {
        Id = id;
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}