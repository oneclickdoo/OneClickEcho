using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

namespace OneClickEcho.Domain.ViberSessionAggregate.Repositories;

public sealed record ViberSessionLatestMessage(Guid SessionId, DateTime At, string? Text);

public interface IViberSessionRepository : IRepository<ViberSession, ViberSessionId>
{
    Task<ViberSession?> GetByIdAsync(ViberSessionId id, CancellationToken cancellationToken = default);

    Task<List<ViberSession>> ListSameHandsetAsync(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<ViberSession?> FindLatestAsync(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<List<ViberSession>> ListByCompanyAsync(
        CompanyId companyId,
        string? phoneSearch,
        string? sender,
        CancellationToken cancellationToken = default);

    Task<List<ViberSessionMessage>> ListMessagesAsync(ViberSessionId sessionId, CancellationToken cancellationToken = default);

    Task<List<ViberSessionLatestMessage>> GetLatestMessagesAsync(
        IReadOnlyCollection<ViberSessionId> sessionIds,
        CancellationToken cancellationToken = default);

    Task<List<long>> GetRecentOutboundMessageIdsAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);

    void Add(ViberSession session);

    void AddMessage(ViberSessionMessage message);

    Task<bool> InboundTokenExistsAsync(string messageToken, CancellationToken cancellationToken = default);

    Task NoteSessionIdAsync(long viberMessageId, string? msisdn, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Opens or refreshes the 24h window for a user reply and stores the inbound text.</summary>
    Task<ViberSession?> FindByOutboundMessageIdAsync(long viberMessageId, CancellationToken cancellationToken = default);

    Task RecordInboundAsync(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        string text,
        string? messageToken,
        DateTime receivedUtc,
        CancellationToken cancellationToken = default);
}
