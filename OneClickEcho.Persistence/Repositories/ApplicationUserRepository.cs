using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.ApplicationUserAggregate;
using OneClickEcho.Domain.ApplicationUserAggregate.Repositories;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class ApplicationUserRepository(ApplicationDbContext dbContext) : IApplicationUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ApplicationUser>()
            .Include(e => e.CompanyIds)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ApplicationUser>()
            .Include(e => e.CompanyIds)
            .FirstOrDefaultAsync(g => g.Email == email, cancellationToken);
    }

    public async Task<List<ApplicationUser>> Search(Guid companyId, string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ApplicationUser>()
            .Where(e => !e.CompanyIds.Any(x => x.CompanyId == CompanyId.Create(companyId)) && e.Email!.Contains(email))
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPagedList<ApplicationUser>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default)
    {
        PagedList<ApplicationUser> pagedList = await PagedList<ApplicationUser>
            .CreateAsync(_dbContext.ApplicationUsers, query, cancellationToken);

        return pagedList;
    }
}