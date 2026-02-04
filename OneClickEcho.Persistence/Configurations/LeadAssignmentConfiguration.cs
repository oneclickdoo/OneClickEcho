using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate.Entities;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class LeadAssignmentConfiguration : EntityTypeConfiguration<LeadAssignment, LeadAssignmentId>
{
    public new void Configure(EntityTypeBuilder<LeadAssignment> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => LeadAssignmentId.Create(value));

        builder
            .HasOne<Lead>()
            .WithMany()
            .HasForeignKey(e => e.LeadId)
            .IsRequired();

        builder
            .HasOne<LeadCollection>()
            .WithMany(e => e.LeadAssignments)
            .HasForeignKey(e => e.LeadCollectionId)
            .IsRequired();
    }
}