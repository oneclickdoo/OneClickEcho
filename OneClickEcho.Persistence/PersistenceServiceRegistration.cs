using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneClickEcho.Persistence.Identity;
using System.Reflection;

namespace OneClickEcho.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
            options.UseOpenIddict();
        });

        services
            .Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddIdentityService(configuration);

        return services;
    }
}