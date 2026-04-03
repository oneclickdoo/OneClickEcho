using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.Common.Identity;

public interface IUserManager
{
    public Task CreateUser(string email, string password, CompanyId companyId, UserRole userRole);

    public Task AssignUser(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default);

    public Task ForceUpdatePassword(Guid userId, string newPassword, CancellationToken cancellationToken = default);

    public Task UpdatePassword(string password, string newPassword, CancellationToken cancellationToken = default);

    public Task DeleteUser(Guid userId, CancellationToken cancellationToken = default);

    public Task<ApplicationUser?> GetCurrentUser();
}