using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ReceivedMessageConfiguration : EntityTypeConfiguration<ReceivedMessage, ReceivedMessageId>
{
    public new void Configure(EntityTypeBuilder<ReceivedMessage> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ReceivedMessageId.Create(value));

        builder
            .HasOne<CampaignLead>()
            .WithMany(e => e.ReceivedMessages)
            .HasForeignKey(c => c.CampaignLeadId)
            .IsRequired();
    }
}