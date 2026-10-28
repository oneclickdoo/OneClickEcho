using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ViberSessionMessageConfiguration : EntityTypeConfiguration<ViberSessionMessage, ViberSessionMessageId>
{
    public new void Configure(EntityTypeBuilder<ViberSessionMessage> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ViberSessionMessageId.Create(value));

        builder.Property(e => e.SessionId)
            .HasConversion(
                id => id.Value,
                value => ViberSessionId.Create(value));

        builder.Property(e => e.ViberMessageId)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 8_000_000_000);

        builder.Property(e => e.Body).HasColumnType("text");

        builder
            .HasOne<ViberSession>()
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ViberMessageId);
        builder.HasIndex(e => e.MessageToken);
    }
}
