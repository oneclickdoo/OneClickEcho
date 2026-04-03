using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Persistence.Configurations.Shared;

public abstract class EntityTypeConfiguration<T, TId> : IEntityTypeConfiguration<T>
    where T : Entity<TId>
    where TId : ValueObject
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
