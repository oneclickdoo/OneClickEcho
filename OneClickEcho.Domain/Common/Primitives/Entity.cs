namespace OneClickEcho.Domain.Common.Primitives;

public abstract class Entity<TId> where TId : ValueObject
{
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }
    public TId Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
}