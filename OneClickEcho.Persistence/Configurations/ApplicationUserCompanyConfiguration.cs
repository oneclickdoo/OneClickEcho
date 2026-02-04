using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Entities;
using OneClickEcho.Domain.ApplicationUserAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Configurations;

public class ApplicationUserCompanyConfiguration
{
    public void Configure(EntityTypeBuilder<ApplicationUserCompany> builder)
    {
        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ApplicationUserCompanyId.Create(value));

        builder.Property(e => e.CompanyId)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => CompanyId.Create(value));

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();

        builder
            .HasOne<ApplicationUser>()
            .WithMany(e => e.CompanyIds)
            .HasForeignKey("ApplicationUserId")
            .IsRequired();
    }
}