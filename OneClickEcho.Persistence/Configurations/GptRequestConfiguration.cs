using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.GptRequestAggregate.ValueObjects;
using OneClickEcho.Domain.NounCaseAggregate;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class GptRequestConfiguration : EntityTypeConfiguration<GptRequest, GptRequestId>
{
    public new void Configure(EntityTypeBuilder<GptRequest> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => GptRequestId.Create(value));

        builder
            .HasOne<NounCase>()
            .WithMany()
            .HasForeignKey(c => c.NounCaseId)
            .IsRequired(false);

        builder
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .IsRequired(false);
    }
}