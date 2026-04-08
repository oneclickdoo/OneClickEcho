using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneClickEcho.Domain.ApplicationUserAggregate;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OneClickEcho.Persistence.Identity;

public static class IdentityServiceRegistration
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                options.UseAspNetCore().DisableTransportSecurityRequirement();

                // Enable the token endpoint.
                options.SetTokenEndpointUris("connect/token");

                // Enable the password flow and refresh token flow.
                options
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                // Accept anonymous clients (i.e clients that don't send a client_id).
                options.AcceptAnonymousClients();

                // Required: token requests are rejected with invalid_scope (400) if a scope is not registered here.
                options.RegisterScopes(
                    Scopes.OpenId,
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Scopes.OfflineAccess);

                // Short opaque tokens for browsers (httpOnly cookies ~4KB limit). Default encrypted JWTs exceed that.
                options.UseReferenceAccessTokens()
                    .UseReferenceRefreshTokens();

                // Development certificates use the local user store and fail in Linux Docker — never use them in a container.
                // Note: ephemeral keys reset on restart (refresh tokens from previous runs become invalid).
                bool inContainer = string.Equals(
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (hostEnvironment.IsDevelopment() && !inContainer)
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Required to resolve reference access tokens against the database on each API request.
                options.EnableTokenEntryValidation();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        return services;
    }
}