using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            Domain.ApplicationUserAggregate.ApplicationUser? user = await userRepository.GetByEmailAsync("itocs@oneclick.rs");

            company = await companyRepository.GetByNameAsync("OneClick Echo");

            if (user is null && company is not null)
            {
                await userManager.CreateUser("itocs@oneclick.rs", "Testing123!", company.Id, UserRole.Administrator);
            }

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            return false;
        }

        return true;
    }
}