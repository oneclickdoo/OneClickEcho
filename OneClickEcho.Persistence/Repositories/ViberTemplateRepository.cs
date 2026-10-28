using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberTemplateAggregate;
using OneClickEcho.Domain.ViberTemplateAggregate.Repositories;
using OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Repositories;

public class ViberTemplateRepository(ApplicationDbContext dbContext) : IViberTemplateRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ViberTemplate?> GetByIdAsync(ViberTemplateId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ViberTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<ViberTemplate>> ListByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ViberTemplates
            .AsNoTracking()
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(ViberTemplate template) => _dbContext.ViberTemplates.Add(template);

    public void Remove(ViberTemplate template) => _dbContext.ViberTemplates.Remove(template);
}
