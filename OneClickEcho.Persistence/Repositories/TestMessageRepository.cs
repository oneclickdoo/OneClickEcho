using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Domain.TestMessageAggregate.Repositories;

namespace OneClickEcho.Persistence.Repositories
{
    public class TestMessageRepository(ApplicationDbContext dbContext) : ITestMessageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<List<TestMessage>> GetViberTestMessagesForLast48Hours(CancellationToken cancellationToken = default)
        {
            List<TestMessage> result = await _dbContext.Set<TestMessage>()
                .Where(x => x.IsViber)
                .Where(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-2) && !x.IsDelivered)
                .ToListAsync(cancellationToken);

            return result;
        }

        public async Task<List<TestMessage>> GetSmsTestMessagesForLast48Hours(CancellationToken cancellationToken = default)
        {
            List<TestMessage> result = await _dbContext.Set<TestMessage>()
                .Where(x => !x.IsViber)
                .Where(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-2) && !x.IsDelivered)
                .ToListAsync(cancellationToken);

            return result;
        }

        public void Add(TestMessage testMessage)
        {
            _dbContext.Set<TestMessage>().Add(testMessage);
        }
    }
}
