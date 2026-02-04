using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class CampaignLeadCollectionConfiguration : EntityTypeConfiguration<CampaignLeadCollection, CampaignLeadCollectionId>
{
    public new void Configure(EntityTypeBuilder<CampaignLeadCollection> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => CampaignLeadCollectionId.Create(value));

        builder
            .HasOne<LeadCollection>()
            .WithMany()
            .HasForeignKey(e => e.LeadCollectionId)
            .IsRequired();

        builder
            .HasOne<Campaign>()
            .WithMany(e => e.LeadCollections)
            .HasForeignKey("CampaignId")
            .IsRequired();
    }
}