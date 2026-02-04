using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class LeadConfiguration : EntityTypeConfiguration<Lead, LeadId>
{
    public new void Configure(EntityTypeBuilder<Lead> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => LeadId.Create(value));

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .IsRequired();

        builder
            .HasMany<CampaignLead>()
            .WithOne()
            .HasForeignKey(c => c.LeadId)
            .IsRequired();

        builder.HasIndex(c => c.PhoneNumber);
    }
}