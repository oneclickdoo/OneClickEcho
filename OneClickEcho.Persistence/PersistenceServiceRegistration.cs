using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneClickEcho.Persistence.Identity;
using OneClickEcho.Persistence.Options;
using System.Reflection;

namespace OneClickEcho.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceService(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
            options.UseOpenIddict();
        });

        services.Configure<CampaignLeadViberMessageIdOptions>(
            configuration.GetSection(CampaignLeadViberMessageIdOptions.SectionName));

        services.AddMemoryCache();

        services
            .Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddIdentityService(configuration, hostEnvironment);

        return services;
    }
}