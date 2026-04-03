using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class CampaignConfiguration : EntityTypeConfiguration<Campaign, CampaignId>
{
    public new void Configure(EntityTypeBuilder<Campaign> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => CampaignId.Create(value));

        builder
            .HasMany<CampaignLead>()
            .WithOne()
            .HasForeignKey(c => c.CampaignId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany<GptRequest>()
            .WithOne()
            .HasForeignKey(c => c.CampaignId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}