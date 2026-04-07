using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OneClickEcho.App.Controllers
{
    // @TODO: Replace with ApiController and move all of the functionality to the Application project
    public class AuthorizationController(SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager, IApplicationUserRepository userRepository) : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IApplicationUserRepository _userRepository = userRepository;

        [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();

            if (request is null)
            {
                return BadRequest("");
            }

            if (request.IsPasswordGrantType())
            {
                // Use Identity's user lookup (normalized email) so the same row/hash is used as after password reset.
#pragma warning disable CS8604 // Possible null reference argument.
                ApplicationUser? user = await _userManager.FindByEmailAsync(request.Username);
#pragma warning restore CS8604 // Possible null reference argument.

                if (user == null)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The username/password couple is invalid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Validate the username/password parameters and ensure the account is not locked out.
#pragma warning disable CS8604 // Possible null reference argument.
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
                    .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
#pragma warning restore CS8604 // Possible null reference argument.

                if (!result.Succeeded)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The username/password couple is invalid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                ApplicationUser? userWithCompanies = await _userRepository.GetByIdAsync(user.Id)
                    ?? user;

                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                ClaimsIdentity identity = new(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                // Add the claims that will be persisted in the tokens.
                identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                        .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                        .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                        .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                        .SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)])
                        .SetClaim("CompanyIds", JsonConvert.SerializeObject(userWithCompanies.CompanyIds.Select(id => id.CompanyId.Value.ToString())), JsonClaimValueTypes.Json);

                identity.SetScopes(new[]
                {
                    Scopes.OpenId,
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Scopes.OfflineAccess,
                }.Intersect(request.GetScopes()));

                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the refresh token.
#pragma warning disable CS8604 // Possible null reference argument.
                ApplicationUser? user = await _userManager.FindByIdAsync(result.Principal.GetClaim(Claims.Subject));
#pragma warning restore CS8604 // Possible null reference argument.

                if (user == null)
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    AuthenticationProperties properties = new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                ClaimsIdentity identity = new(result.Principal.Claims,
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                // Override the user claims present in the principal in case they changed since the refresh token was issued.
                identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                        .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                        .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                        .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
                        .SetClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);
                // .SetClaim("CompanyId", user.CompanyId.Value.ToString());

                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name or Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

#pragma warning disable CS8604 // Possible null reference argument.
                    if (claim.Subject.HasScope(Scopes.Profile))
                    {
                        yield return Destinations.IdentityToken;
                    }
#pragma warning restore CS8604 // Possible null reference argument.

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

#pragma warning disable CS8604 // Possible null reference argument.
                    if (claim.Subject.HasScope(Scopes.Email))
                    {
                        yield return Destinations.IdentityToken;
                    }
#pragma warning restore CS8604 // Possible null reference argument.

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

#pragma warning disable CS8604 // Possible null reference argument.
                    if (claim.Subject.HasScope(Scopes.Roles))
                    {
                        yield return Destinations.IdentityToken;
                    }
#pragma warning restore CS8604 // Possible null reference argument.

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
