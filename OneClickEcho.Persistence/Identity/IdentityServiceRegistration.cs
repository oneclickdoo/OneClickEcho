using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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

                // Password flow + no client_id: OpenIddict otherwise requires DB permissions linking a client to
                // each scope and grant type (would reject every token request after scopes exist in OpenIddictScopes).
                options.IgnoreScopePermissions();
                options.IgnoreGrantTypePermissions();

                // Token requests fail if a requested scope is not registered here (names must match the client).
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
                // Ephemeral keys: new key material per process → load-balanced APIs or restarts invalidate access/refresh tokens
                // and reference-token crypto may not match between instances (401 on /api/User/CurrentUser right after login).
                bool inContainer = string.Equals(
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (hostEnvironment.IsDevelopment() && !inContainer)
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else if (TryGetPersistedSymmetricKeys(configuration, out byte[]? signingKey, out byte[]? encryptionKey))
                {
                    options.AddSigningKey(new SymmetricSecurityKey(signingKey));
                    options.AddEncryptionKey(new SymmetricSecurityKey(encryptionKey));
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

    /// <summary>
    /// Base64 keys (>= 32 bytes decoded). Set <c>OpenIddict:SigningKey</c> and optionally <c>OpenIddict:EncryptionKey</c>
    /// (env <c>OpenIddict__SigningKey</c>). Generate: <c>openssl rand -base64 32</c>.
    /// </summary>
    private static bool TryGetPersistedSymmetricKeys(
        IConfiguration configuration,
        out byte[]? signingKey,
        out byte[]? encryptionKey)
    {
        signingKey = null;
        encryptionKey = null;

        string? signingB64 = configuration["OpenIddict:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingB64))
        {
            return false;
        }

        // .env / copy-paste often adds newlines or accidental quotes — normalize before base64 decode.
        signingB64 = signingB64.Trim().Trim('"', '\'').Replace("\r", "").Replace("\n", "");

        try
        {
            signingKey = Convert.FromBase64String(signingB64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("OpenIddict:SigningKey must be valid base64.");
        }

        if (signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "OpenIddict:SigningKey must decode to at least 32 bytes (use: openssl rand -base64 32).");
        }

        string? encryptionB64 = configuration["OpenIddict:EncryptionKey"];
        if (string.IsNullOrWhiteSpace(encryptionB64))
        {
            encryptionKey = signingKey;
            return true;
        }

        encryptionB64 = encryptionB64.Trim().Trim('"', '\'').Replace("\r", "").Replace("\n", "");

        try
        {
            encryptionKey = Convert.FromBase64String(encryptionB64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("OpenIddict:EncryptionKey must be valid base64.");
        }

        if (encryptionKey.Length < 32)
        {
            throw new InvalidOperationException(
                "OpenIddict:EncryptionKey must decode to at least 32 bytes (use: openssl rand -base64 32).");
        }

        return true;
    }
}