using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.TestMessageAggregate.ValueObjects;

namespace OneClickEcho.Domain.TestMessageAggregate.Repositories;

public interface ITestMessageRepository : IRepository<TestMessage, TestMessageId>
{
    public Task<List<TestMessage>> GetViberTestMessagesForLast48Hours(CancellationToken cancellationToken = default);
    
    public Task<List<TestMessage>> GetSmsTestMessagesForLast48Hours(CancellationToken cancellationToken = default);
    
    public void Add(TestMessage testMessage);
}