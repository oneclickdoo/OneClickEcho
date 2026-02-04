using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations
{
    public class SenderConfiguration : EntityTypeConfiguration<Sender, SenderId>
    {
        public new void Configure(EntityTypeBuilder<Sender> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => SenderId.Create(value));

            builder
            .HasOne<Company>()
            .WithMany(e => e.Senders)
            .HasForeignKey(c => c.CompanyId)
            .IsRequired();
        }
    }
}
