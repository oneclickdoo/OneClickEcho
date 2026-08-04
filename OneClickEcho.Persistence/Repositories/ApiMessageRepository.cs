using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories
{
    public class ApiMessageRepository(ApplicationDbContext dbContext) : IApiMessageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<ApiMessage?> GetByIdAsync(ApiMessageId id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ApiMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<IPagedList<ApiMessage>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default)
        {
            PagedList<ApiMessage> apiMessagePagedList = await PagedList<ApiMessage>
                .CreateAsync(_dbContext.ApiMessages, query, cancellationToken);

            return apiMessagePagedList;
        }

        public async Task<List<ApiMessage>> GetUnsentApiMessages(DateTime startDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ApiMessages
                .Where(m => !m.IsSent)
                .Where(m => m.CreatedAt.ToUniversalTime() > startDate.ToUniversalTime())
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ApiMessage>> GetSentApiMessages(DateTime startDate, CancellationToken cancellationToken = default)
        {
            // Keep polling until terminal Clicked / Expired (same idea as campaign leads).
            // Stopping at Seen prevented ClickCount updates that arrive after the message was opened.
            return await _dbContext.ApiMessages
                .Where(m => m.IsSent && m.CreatedAt.ToUniversalTime() >= startDate.ToUniversalTime())
                .Where(m =>
                    m.ViberStatus != CampaignLeadViberStatus.Clicked &&
                    m.ViberStatus != CampaignLeadViberStatus.Expired)
                .ToListAsync(cancellationToken);
        }

        public async Task<ApiMessage?> FindIdenticalSameDayAsync(
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
            CancellationToken cancellationToken = default)
        {
            string phone = phoneNumber.Trim();
            string text = message.Trim();
            string senderNorm = NormalizeOptional(sender) ?? string.Empty;
            string mediaNorm = NormalizeOptional(viberMedia) ?? string.Empty;
            string buttonUrlNorm = NormalizeOptional(viberButtonUrl) ?? string.Empty;
            string buttonTitleNorm = NormalizeOptional(viberButtonUrlTitle) ?? string.Empty;

            return await _dbContext.ApiMessages
                .AsNoTracking()
                .Where(m => m.CompanyId == companyId)
                .Where(m => m.CreatedAt >= dayStartUtc && m.CreatedAt < dayEndUtc)
                .Where(m => m.PhoneNumber == phone)
                .Where(m => m.MessageType == messageType)
                .Where(m => m.Message == text)
                .Where(m => (m.Sender ?? string.Empty) == senderNorm)
                .Where(m => (m.ViberMedia ?? string.Empty) == mediaNorm)
                .Where(m => (m.ViberButtonUrl ?? string.Empty) == buttonUrlNorm)
                .Where(m => (m.ViberButtonUrlTitle ?? string.Empty) == buttonTitleNorm)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public void Add(ApiMessage apiMessage)
        {
            _dbContext.Set<ApiMessage>().Add(apiMessage);
        }

        private static string? NormalizeOptional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
