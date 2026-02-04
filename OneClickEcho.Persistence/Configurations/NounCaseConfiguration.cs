using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.NounCaseAggregate;
using OneClickEcho.Domain.NounCaseAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class NounCaseConfiguration : EntityTypeConfiguration<NounCase, NounCaseId>
{
    public new void Configure(EntityTypeBuilder<NounCase> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => NounCaseId.Create(value));

        builder
            .HasIndex(e => e.Nominative)
            .IsUnique();

        builder
            .HasMany<GptRequest>()
            .WithOne()
            .HasForeignKey(c => c.NounCaseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}