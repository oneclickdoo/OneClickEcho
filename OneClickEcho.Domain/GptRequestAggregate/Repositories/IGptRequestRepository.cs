using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.GptRequestAggregate.ValueObjects;

namespace OneClickEcho.Domain.GptRequestAggregate.Repositories;

public interface IGptRequestRepository : IRepository<GptRequest, GptRequestId>
{
    Task<GptRequest?> GetByIdAsync(GptRequestId id, CancellationToken cancellationToken = default);
    void Add(GptRequest gptRequest);
}