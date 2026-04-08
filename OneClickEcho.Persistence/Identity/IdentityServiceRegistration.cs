using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
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

                // When /connect/token is called as http://api:3901/... (Docker), OpenIddict defaults issuer to that URL.
                // Validation then rejects tokens with ID2088 if expected issuer differs. Set a stable public issuer
                // (same host users use in the browser). UseLocalServer() copies this into validation options.
                if (TryGetConfiguredIssuer(configuration, out Uri? configuredIssuer))
                {
                    options.SetIssuer(configuredIssuer);
                }

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
                else if (TryGetPersistedRsaSigningKey(configuration, out RSAParameters rsaParameters, out string rsaKeyId,
                             out byte[] rsaPkcs8Der))
                {
                    // OpenIddict 5+ requires an asymmetric signing key; symmetric keys are not accepted for signing.
                    options.AddSigningKey(new RsaSecurityKey(rsaParameters) { KeyId = rsaKeyId });
                    byte[] encryptionKey = ResolveSymmetricEncryptionKey(configuration, rsaPkcs8Der);
                    options.AddEncryptionKey(new SymmetricSecurityKey(encryptionKey));
                }
                else if (OpenIddictSymmetricSecretsConfiguredWithoutRsa(configuration))
                {
                    throw new InvalidOperationException(
                        "OpenIddict 5+ requires an RSA private key for token signing. Set OpenIddict:RsaPrivateKeyPkcs8 " +
                        "(PKCS#8 private key, DER, base64). Keep OpenIddict:SigningKey and/or OpenIddict:EncryptionKey " +
                        "for symmetric encryption across replicas, or omit them to derive encryption from the RSA key. " +
                        "Generate: openssl genrsa 2048 | openssl pkcs8 -topk8 -nocrypt -outform DER | openssl base64 -A");
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
    /// RSA signing: <c>OpenIddict:RsaPrivateKeyPkcs8</c> (env <c>OpenIddict__RsaPrivateKeyPkcs8</c>) — PKCS#8 DER, base64.
    /// Optional <c>OpenIddict:RsaKeyId</c> for key id header (default <c>rsa-1</c>).
    /// Symmetric encryption: <c>OpenIddict:EncryptionKey</c>, else legacy <c>OpenIddict:SigningKey</c>, else SHA256(RSA PKCS#8)[0..32].
    /// </summary>
    private static bool TryGetPersistedRsaSigningKey(
        IConfiguration configuration,
        out RSAParameters rsaParameters,
        out string keyId,
        out byte[] pkcs8Der)
    {
        rsaParameters = default;
        keyId = "rsa-1";
        pkcs8Der = Array.Empty<byte>();

        string? pkcs8B64 = configuration["OpenIddict:RsaPrivateKeyPkcs8"];
        if (string.IsNullOrWhiteSpace(pkcs8B64))
        {
            return false;
        }

        pkcs8B64 = NormalizeBase64Config(pkcs8B64);

        try
        {
            pkcs8Der = Convert.FromBase64String(pkcs8B64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("OpenIddict:RsaPrivateKeyPkcs8 must be valid base64 (PKCS#8 DER).");
        }

        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(pkcs8Der, out _);
            rsaParameters = rsa.ExportParameters(true);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "OpenIddict:RsaPrivateKeyPkcs8 must be a valid PKCS#8 private key (DER).", ex);
        }

        string? kid = configuration["OpenIddict:RsaKeyId"];
        if (!string.IsNullOrWhiteSpace(kid))
        {
            keyId = kid.Trim();
        }

        return true;
    }

    private static bool OpenIddictSymmetricSecretsConfiguredWithoutRsa(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["OpenIddict:RsaPrivateKeyPkcs8"]))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(configuration["OpenIddict:SigningKey"])
               || !string.IsNullOrWhiteSpace(configuration["OpenIddict:EncryptionKey"]);
    }

    private static byte[] ResolveSymmetricEncryptionKey(IConfiguration configuration, ReadOnlySpan<byte> rsaPkcs8Der)
    {
        if (TryDecodeSymmetricKey(configuration["OpenIddict:EncryptionKey"], out byte[]? enc))
        {
            return enc;
        }

        if (TryDecodeSymmetricKey(configuration["OpenIddict:SigningKey"], out byte[]? legacy))
        {
            return legacy;
        }

        byte[] derived = new byte[32];
        SHA256.HashData(rsaPkcs8Der, derived);
        return derived;
    }

    private static bool TryDecodeSymmetricKey(string? base64, out byte[] key)
    {
        key = Array.Empty<byte>();
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        base64 = NormalizeBase64Config(base64);

        try
        {
            key = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("OpenIddict symmetric key must be valid base64.");
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                "OpenIddict symmetric key must decode to at least 32 bytes (use: openssl rand -base64 32).");
        }

        return true;
    }

    private static string NormalizeBase64Config(string value) =>
        value.Trim().Trim('"', '\'').Replace("\r", "").Replace("\n", "");

    /// <summary>Optional. Public base URL of the auth server as seen by clients (e.g. <c>https://viber.oneclick.rs</c>).
    /// Env: <c>OpenIddict__Issuer</c>.</summary>
    private static bool TryGetConfiguredIssuer(IConfiguration configuration, [NotNullWhen(true)] out Uri? issuerUri)
    {
        issuerUri = null;
        string? raw = configuration["OpenIddict:Issuer"]?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            return false;
        }

        raw = raw.TrimEnd('/');
        if (!Uri.TryCreate(raw + "/", UriKind.Absolute, out Uri? parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "OpenIddict:Issuer must be an absolute http(s) URL (e.g. https://viber.oneclick.rs).");
        }

        issuerUri = parsed;
        return true;
    }
}