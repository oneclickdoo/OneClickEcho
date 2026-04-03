using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class CampaignLeadConfiguration : EntityTypeConfiguration<CampaignLead, CampaignLeadId>
{
    public new void Configure(EntityTypeBuilder<CampaignLead> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => CampaignLeadId.Create(value));

        builder.Property(e => e.ViberMessageId)
            .ValueGeneratedOnAdd();

        builder
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .IsRequired();

        builder
            .HasOne<Lead>()
            .WithMany()
            .HasForeignKey(c => c.LeadId)
            .IsRequired();

        builder
            .HasMany(e => e.ReceivedMessages)
            .WithOne()
            .HasForeignKey(e => e.CampaignLeadId)
            .IsRequired();
    }
}