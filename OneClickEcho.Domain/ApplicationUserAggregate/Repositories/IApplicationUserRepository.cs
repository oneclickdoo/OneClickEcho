using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;

namespace OneClickEcho.Domain.ApplicationUserAggregate.Repositories;

public interface IApplicationUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<List<ApplicationUser>> Search(Guid companyId, string email, CancellationToken cancellationToken = default);

    Task<IPagedList<ApplicationUser>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default);
}