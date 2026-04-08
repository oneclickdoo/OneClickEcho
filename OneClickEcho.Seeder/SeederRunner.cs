using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Persistence;

namespace OneClickEcho.Seeder;

public class SeederRunner
{
    public static async Task<bool> Execute(IServiceProvider app)
    {
        using IServiceScope scope = app.CreateScope();

        IServiceProvider services = scope.ServiceProvider;

        ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();

        ILogger<SeederRunner> logger = services.GetRequiredService<ILogger<SeederRunner>>();

        logger.LogInformation("Migration started.");

        try
        {
            logger.LogInformation("Database creation started.");

            await dbContext.Database.MigrateAsync();

            // @TODO: This code is disgusting.
            logger.LogInformation("Seeding database started.");

            // OpenIddict + EF Core: scope validation uses OpenIddictScopes rows. RegisterScopes() in code does not insert them.
            IOpenIddictScopeManager openIddictScopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
            foreach (string scopeName in new[]
                     {
                         OpenIddictConstants.Scopes.OpenId,
                         OpenIddictConstants.Scopes.Email,
                         OpenIddictConstants.Scopes.Profile,
                         OpenIddictConstants.Scopes.Roles,
                         OpenIddictConstants.Scopes.OfflineAccess
                     })
            {
                if (await openIddictScopeManager.FindByNameAsync(scopeName) is null)
                {
                    OpenIddictScopeDescriptor descriptor = new()
                    {
                        Name = scopeName,
                        DisplayName = scopeName
                    };

                    await openIddictScopeManager.CreateAsync(descriptor);
                    logger.LogInformation("Registered OpenIddict scope: {Scope}", scopeName);
                }
            }

            RoleManager<IdentityRole<Guid>> roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (string roleName in UserRoles.GetAllUserRoles())
            {
                bool roleExist = await roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            ICompanyRepository companyRepository = services.GetRequiredService<ICompanyRepository>();

            Company? company = await companyRepository.GetByNameAsync("OneClick Echo");

            if (company is null)
            {
                companyRepository.Add(new Company("OneClick Echo", "sms-username-dev", "sms-password-dev"));
            }

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            IApplicationUserRepository userRepository = services.GetRequiredService<IApplicationUserRepository>();

            IUserManager userManager = services.GetRequiredService<IUserManager>();

            ApplicationUser? user = await userRepository.GetByEmailAsync("itocs@oneclick.rs");

            company = await companyRepository.GetByNameAsync("OneClick Echo");

            if (user is null && company is not null)
            {
                await userManager.CreateUser("itocs@oneclick.rs", "Testing123!", company.Id, UserRole.Administrator);
            }

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            // One-time ops: set RESET_ITOCS_PASSWORD on the API container, restart once, then remove it.
            string? resetItocsPassword = Environment.GetEnvironmentVariable("RESET_ITOCS_PASSWORD");
            if (!string.IsNullOrWhiteSpace(resetItocsPassword))
            {
                // Visible at startup; use: docker compose logs api --since 2m | grep RESET
                logger.LogInformation(
                    "RESET_ITOCS_PASSWORD is set (password length {Length}); running one-time reset for itocs@oneclick.rs.",
                    resetItocsPassword.Length);

                UserManager<ApplicationUser> identityUserManager =
                    services.GetRequiredService<UserManager<ApplicationUser>>();

                ApplicationUser? itocsUser = await identityUserManager.FindByEmailAsync("itocs@oneclick.rs");

                if (itocsUser is null)
                {
                    logger.LogWarning("RESET_ITOCS_PASSWORD is set but no user itocs@oneclick.rs exists.");
                }
                else
                {
                    string resetToken = await identityUserManager.GeneratePasswordResetTokenAsync(itocsUser);
                    IdentityResult pwResult =
                        await identityUserManager.ResetPasswordAsync(itocsUser, resetToken, resetItocsPassword);

                    if (!pwResult.Succeeded)
                    {
                        logger.LogWarning(
                            "RESET_ITOCS_PASSWORD: ResetPasswordAsync failed, trying RemovePassword + AddPassword. Errors: {Errors}",
                            string.Join("; ", pwResult.Errors.Select(e => $"{e.Code}: {e.Description}")));

                        IdentityResult removeResult = await identityUserManager.RemovePasswordAsync(itocsUser);
                        pwResult = removeResult.Succeeded
                            ? await identityUserManager.AddPasswordAsync(itocsUser, resetItocsPassword)
                            : removeResult;

                        if (!pwResult.Succeeded)
                        {
                            logger.LogError("RESET_ITOCS_PASSWORD failed: {Errors}",
                                string.Join("; ", pwResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
                        }
                    }

                    if (pwResult.Succeeded)
                    {
                        await identityUserManager.SetLockoutEndDateAsync(itocsUser, null);
                        await identityUserManager.ResetAccessFailedCountAsync(itocsUser);
                        itocsUser.EmailConfirmed = true;
                        IdentityResult updateResult = await identityUserManager.UpdateAsync(itocsUser);
                        if (!updateResult.Succeeded)
                        {
                            logger.LogError("RESET_ITOCS_PASSWORD: UpdateAsync after reset failed: {Errors}",
                                string.Join("; ", updateResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
                        }

                        await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
                        logger.LogInformation(
                            "ITOCS_PASSWORD_RESET_OK: itocs@oneclick.rs password was saved. Remove RESET_ITOCS_PASSWORD and restart API.");
                        logger.LogWarning(
                            "Password for itocs@oneclick.rs was reset. Remove RESET_ITOCS_PASSWORD from the environment immediately.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            return false;
        }

        return true;
    }
}