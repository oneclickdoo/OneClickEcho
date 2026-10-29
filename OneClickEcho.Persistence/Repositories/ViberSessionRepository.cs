using Microsoft.EntityFrameworkCore;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberSessionAggregate;
using OneClickEcho.Domain.ViberSessionAggregate.Entities;
using OneClickEcho.Domain.ViberSessionAggregate.Enums;
using OneClickEcho.Domain.ViberSessionAggregate.Repositories;
using OneClickEcho.Domain.ViberSessionAggregate.ValueObjects;

namespace OneClickEcho.Persistence.Repositories;

public class ViberSessionRepository(ApplicationDbContext dbContext) : IViberSessionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ViberSession?> GetByIdAsync(ViberSessionId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ViberSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<ViberSession?> FindLatestAsync(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        string phone = ViberPhone.Normalize(phoneNumber);
        string senderNorm = sender.Trim();
        return await _dbContext.ViberSessions
            .Where(s => s.CompanyId == companyId && s.Sender == senderNorm && s.PhoneNumber == phone)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ViberSession>> ListByCompanyAsync(
        CompanyId companyId,
        string? phoneSearch,
        string? sender,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ViberSession> query = _dbContext.ViberSessions
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(sender))
        {
            string senderNorm = sender.Trim();
            query = query.Where(s => s.Sender == senderNorm);
        }

        if (!string.IsNullOrWhiteSpace(phoneSearch))
        {
            string digits = new string(phoneSearch.Where(char.IsDigit).ToArray());
            if (digits.Length > 0)
            {
                string alternate = digits.StartsWith('0') && digits.Length > 1
                    ? "381" + digits[1..]
                    : digits.StartsWith("381", StringComparison.Ordinal) && digits.Length > 3
                        ? "0" + digits[3..]
                        : digits;
                query = query.Where(s => s.PhoneNumber.Contains(digits) || s.PhoneNumber.Contains(alternate));
            }
        }

        return await query
            .OrderByDescending(s => s.LastInboundAt ?? s.StartedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ViberSessionLatestMessage>> GetLatestMessagesAsync(
        IReadOnlyCollection<ViberSessionId> sessionIds,
        CancellationToken cancellationToken = default)
    {
        if (sessionIds.Count == 0)
        {
            return [];
        }

        List<ViberSessionMessage> messages = await _dbContext.ViberSessionMessages
            .AsNoTracking()
            .Where(m => sessionIds.Contains(m.SessionId))
            .ToListAsync(cancellationToken);

        return messages
            .GroupBy(m => m.SessionId.Value)
            .Select(group =>
            {
                ViberSessionMessage latest = group.OrderByDescending(m => m.CreatedAt).First();
                string? text = string.IsNullOrWhiteSpace(latest.Body) ? latest.Media : latest.Body;
                return new ViberSessionLatestMessage(group.Key, latest.CreatedAt, text);
            })
            .ToList();
    }

    public async Task<List<ViberSessionMessage>> ListMessagesAsync(ViberSessionId sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ViberSessionMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<long>> GetRecentOutboundMessageIdsAsync(DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ViberSessionMessages
            .AsNoTracking()
            .Where(m => m.Direction == ViberSessionDirection.Out && m.CreatedAt >= sinceUtc && m.ViberMessageId > 0)
            .Select(m => m.ViberMessageId)
            .ToListAsync(cancellationToken);
    }

    public void Add(ViberSession session) => _dbContext.ViberSessions.Add(session);

    public void AddMessage(ViberSessionMessage message) => _dbContext.ViberSessionMessages.Add(message);

    public Task<bool> InboundTokenExistsAsync(string messageToken, CancellationToken cancellationToken = default)
    {
        return _dbContext.ViberSessionMessages.AnyAsync(
            m => m.MessageToken == messageToken,
            cancellationToken);
    }

    public async Task NoteSessionIdAsync(long viberMessageId, string? msisdn, string sessionId, CancellationToken cancellationToken = default)
    {
        ViberSessionMessage? message = await _dbContext.ViberSessionMessages
            .FirstOrDefaultAsync(m => m.ViberMessageId == viberMessageId, cancellationToken);

        if (message is not null)
        {
            ViberSession? owner = await _dbContext.ViberSessions
                .FirstOrDefaultAsync(s => s.Id == message.SessionId, cancellationToken);
            if (owner is not null)
            {
                owner.ComtradeSessionId = sessionId;
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(msisdn))
        {
            return;
        }

        string phone = ViberPhone.Normalize(msisdn);
        ViberSession? byPhone = await _dbContext.ViberSessions
            .Where(s => s.PhoneNumber == phone)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (byPhone is not null)
        {
            byPhone.ComtradeSessionId = sessionId;
        }
    }

    public async Task<ViberSession?> FindByOutboundMessageIdAsync(long viberMessageId, CancellationToken cancellationToken = default)
    {
        ViberSessionMessage? message = await _dbContext.ViberSessionMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ViberMessageId == viberMessageId && m.Direction == ViberSessionDirection.Out, cancellationToken);
        if (message is null)
        {
            return null;
        }

        return await _dbContext.ViberSessions.FirstOrDefaultAsync(s => s.Id == message.SessionId, cancellationToken);
    }

    public async Task RecordInboundAsync(
        CompanyId companyId,
        string sender,
        string phoneNumber,
        string text,
        string? messageToken,
        DateTime receivedUtc,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(messageToken) && await InboundTokenExistsAsync(messageToken, cancellationToken))
        {
            return;
        }

        string phone = ViberPhone.Normalize(phoneNumber);
        string senderNorm = sender.Trim();
        DateTime received = receivedUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(receivedUtc, DateTimeKind.Utc)
            : receivedUtc.ToUniversalTime();

        ViberSession? session = await FindLatestAsync(companyId, senderNorm, phone, cancellationToken);
        if (session is null)
        {
            session = new ViberSession(companyId, senderNorm, phone, received);
            session.OpenFromInbound(received);
            Add(session);
        }
        else if (session.LastInboundAt is null || session.ExpiresAt <= received)
        {
            session.OpenFromInbound(received);
        }
        else
        {
            session.ConsecutiveOutbound = 0;
            session.LastInboundAt = received;
        }

        ViberSessionMessage inbound = new(
            session.Id,
            ViberSessionDirection.In,
            0,
            text,
            media: null,
            surveyOptionsJson: null,
            billedAsPromotion: false)
        {
            MessageToken = string.IsNullOrWhiteSpace(messageToken) ? null : messageToken,
            IsSent = true
        };
        AddMessage(inbound);
    }
}
