using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneClickEcho.Domain.ApplicationUserAggregate;

namespace OneClickEcho.Persistence.Configurations;

public class ApplicationUserConfiguration
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // None
    }
}