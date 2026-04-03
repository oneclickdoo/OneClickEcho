using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class CompanyConfiguration : EntityTypeConfiguration<Company, CompanyId>
{
    public new void Configure(EntityTypeBuilder<Company> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => CompanyId.Create(value));

        builder
            .HasMany<Campaign>()
            .WithOne()
            .HasForeignKey(c => c.CompanyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}