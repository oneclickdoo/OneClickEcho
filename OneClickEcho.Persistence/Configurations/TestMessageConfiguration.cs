using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Domain.TestMessageAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations
{
    public class TestMessageConfiguration : EntityTypeConfiguration<TestMessage, TestMessageId>
    {
        public new void Configure(EntityTypeBuilder<TestMessage> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => TestMessageId.Create(value));
            
            builder
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey(c => c.CompanyId)
                .IsRequired();
        }
    }
}
