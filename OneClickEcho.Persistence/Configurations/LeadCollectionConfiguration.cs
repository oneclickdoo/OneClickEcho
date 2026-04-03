using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class LeadCollectionConfiguration : EntityTypeConfiguration<LeadCollection, LeadCollectionId>
{
    public new void Configure(EntityTypeBuilder<LeadCollection> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => LeadCollectionId.Create(value));

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .IsRequired();
    }
}