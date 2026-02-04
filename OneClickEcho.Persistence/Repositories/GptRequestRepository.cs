using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.GptRequestAggregate.Repositories;
using OneClickEcho.Domain.GptRequestAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Repositories;

public class GptRequestRepository(ApplicationDbContext dbContext) : IGptRequestRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<GptRequest?> GetByIdAsync(GptRequestId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<GptRequest>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public void Add(GptRequest gptRequest)
    {
        _dbContext.Set<GptRequest>().Add(gptRequest);
    }
}