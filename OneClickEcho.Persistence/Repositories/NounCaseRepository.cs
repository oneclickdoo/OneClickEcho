using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.NounCaseAggregate;
using OneClickEcho.Domain.NounCaseAggregate.Repositories;

namespace OneClickEcho.Persistence.Repositories;

public class NounCaseRepository : INounCaseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public NounCaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NounCase?> GetByNominativeAsync(string nominative, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<NounCase>()
            .FirstOrDefaultAsync(g => g.Nominative == nominative, cancellationToken);
    }

    public void Add(NounCase nounCase)
    {
        _dbContext.Set<NounCase>().Add(nounCase);
    }
}