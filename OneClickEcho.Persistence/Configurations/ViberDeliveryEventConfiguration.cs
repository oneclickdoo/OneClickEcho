using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ViberDeliveryEventConfiguration : EntityTypeConfiguration<ViberDeliveryEvent, ViberDeliveryEventId>
{
    public new void Configure(EntityTypeBuilder<ViberDeliveryEvent> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ViberDeliveryEventId.Create(value));

        builder
            .HasOne<CampaignLead>()
            .WithMany(e => e.ViberDeliveryEvents)
            .HasForeignKey(e => e.CampaignLeadId)
            .IsRequired();

        builder.HasIndex(e => e.CampaignLeadId);
        builder.HasIndex(e => e.ViberMessageId);
    }
}
