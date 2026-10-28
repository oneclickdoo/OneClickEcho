using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.ViberTemplateAggregate;
using OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ViberTemplateConfiguration : EntityTypeConfiguration<ViberTemplate, ViberTemplateId>
{
    public new void Configure(EntityTypeBuilder<ViberTemplate> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ViberTemplateId.Create(value));

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();

        builder.Property(e => e.Body).HasColumnType("text");
        builder.HasIndex(e => new { e.CompanyId, e.ComtradeId });
    }
}
