using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OneClickEcho.Domain.Common.Identity;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Identity;

public class UserManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork, IApplicationUserRepository applicationUserRepository) : IUserManager
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IApplicationUserRepository _applicationUserRepository = applicationUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task CreateUser(string email, string password, CompanyId companyId, UserRole userRole)
    {
        // @TODO: Check if user already exists
        // Create new User
        ApplicationUser user = new()
        {
            UserName = email,
            Email = email
        };

        IdentityResult identityResult = await _userManager.CreateAsync(user, password);

        // @TODO: Handle this case better (and Exceptions in general). Happens if user already exists.
        if (!identityResult.Succeeded)
        {
            throw new Exception(identityResult.Errors.ToString());
        }

        // Assign role to it
        await _userManager.AddToRoleAsync(user, userRole.ToString());

        // Assign company to it
        user.AddCompany(companyId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AssignUser(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default)
    {
        ApplicationUser user = await _applicationUserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ApplicationException($"User with id {userId} not found");

        // Assign company to user
        user.AddCompany(companyId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ForceUpdatePassword(Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _applicationUserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ApplicationException($"User with id {userId} not found");

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!passwordChangeResult.Succeeded)
        {
            throw new Exception(passwordChangeResult.Errors.ToString());
        }
    }

    public async Task UpdatePassword(string password, string newPassword, CancellationToken cancellationToken)
    {
        ApplicationUser user = await GetCurrentUser()
            ?? throw new ApplicationException($"User not found");

        IdentityResult passwordChangeResult = await _userManager.ChangePasswordAsync(user, password, newPassword);

        if (!passwordChangeResult.Succeeded)
        {
            throw new Exception(passwordChangeResult.Errors.ToString());
        }
    }

    public async Task DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _applicationUserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new ApplicationException($"User with id {userId} not found");

        await _userManager.DeleteAsync(user);
    }

    public async Task<ApplicationUser?> GetCurrentUser()
    {
        System.Security.Claims.ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;

        if (user is not null)
        {
            string? signedInUserId = user.FindFirst("sub")?.Value;

            if (signedInUserId is not null && Guid.TryParse(signedInUserId, out Guid userId))
            {
                ApplicationUser? applicationUser = await _applicationUserRepository.GetByIdAsync(userId);

                return applicationUser;
            }
        }

        return null;
    }
}