using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Persistence.Configurations.Shared;

namespace OneClickEcho.Persistence.Configurations;

public class ApiMessageConfiguration : EntityTypeConfiguration<ApiMessage, ApiMessageId>
{
    public new void Configure(EntityTypeBuilder<ApiMessage> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ApiMessageId.Create(value));
        builder.Property(e => e.ViberMessageId)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 5_000_000_000);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .IsRequired();
    }
}