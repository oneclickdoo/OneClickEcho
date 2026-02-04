using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;

namespace OneClickEcho.Domain.ApiMessageAggregate.Repositories;

public interface IApiMessageRepository : IRepository<ApiMessage, ApiMessageId>
{
    Task<IPagedList<ApiMessage>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default);
    Task<List<ApiMessage>> GetUnsentApiMessages(DateTime startDate, CancellationToken cancellationToken = default);
    Task<List<ApiMessage>> GetSentApiMessages(DateTime startDate, CancellationToken cancellationToken = default);
    void Add(ApiMessage apiMessage);
}
