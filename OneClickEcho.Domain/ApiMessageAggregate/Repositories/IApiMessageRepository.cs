using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.ApiMessageAggregate.Repositories;

public interface IApiMessageRepository : IRepository<ApiMessage, ApiMessageId>
{
    Task<ApiMessage?> GetByIdAsync(ApiMessageId id, CancellationToken cancellationToken = default);
    Task<IPagedList<ApiMessage>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default);
    Task<List<ApiMessage>> GetUnsentApiMessages(DateTime startDate, CancellationToken cancellationToken = default);
    Task<List<ApiMessage>> GetSentApiMessages(DateTime startDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same company + phone + channel + content (text/media/button/sender) within [dayStartUtc, dayEndUtc).
    /// </summary>
    Task<ApiMessage?> FindIdenticalSameDayAsync(
        CompanyId companyId,
        string phoneNumber,
        string message,
        ApiMessageType messageType,
        string? sender,
        string? viberMedia,
        string? viberButtonUrl,
        string? viberButtonUrlTitle,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        CancellationToken cancellationToken = default);

    void Add(ApiMessage apiMessage);
}
