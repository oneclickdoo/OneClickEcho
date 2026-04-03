using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CompanyAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Repositories
{
    public class SenderRepository(ApplicationDbContext dbContext) : ISenderRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Sender?> GetByIdAsync(SenderId id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Sender>()
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<List<Sender>> GetAllByCompanyIdAsync(CompanyId companyId, CancellationToken cancellationToken = default)
        {
            List<Sender> result = await _dbContext.Set<Sender>()
                .Where(x => x.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            return result;
        }

        public void Add(Sender sender)
        {
            _dbContext.Set<Sender>().Add(sender);
        }

        public void Delete(Sender sender)
        {
            _dbContext.Set<Sender>().Remove(sender);
        }
    }
}
