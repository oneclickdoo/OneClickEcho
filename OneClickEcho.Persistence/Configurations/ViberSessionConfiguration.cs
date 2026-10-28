using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ViberSessionConfiguration : EntityTypeConfiguration<ViberSession, ViberSessionId>
{
    public new void Configure(EntityTypeBuilder<ViberSession> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ViberSessionId.Create(value));

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();

        builder.HasIndex(e => new { e.CompanyId, e.Sender, e.PhoneNumber });
    }
}
