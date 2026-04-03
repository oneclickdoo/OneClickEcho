using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories
{
    public class ApiMessageRepository(ApplicationDbContext dbContext) : IApiMessageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<IPagedList<ApiMessage>> GetPagedAsync(IPagedQuery query, CancellationToken cancellationToken = default)
        {
            PagedList<ApiMessage> pagedList = await PagedList<ApiMessage>
                .CreateAsync(_dbContext.ApiMessages, query, cancellationToken);

            return pagedList;
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
            return await _dbContext.ApiMessages
                .Where(m => m.IsSent && m.CreatedAt.ToUniversalTime() >= startDate.ToUniversalTime())
                .Where(m => m.ViberStatus != CampaignLeadViberStatus.Seen)
                .Where(m => m.SMSStatus != CampaignLeadSMSStatus.Delivered)
                .ToListAsync(cancellationToken);
        }

        public void Add(ApiMessage apiMessage)
        {
            _dbContext.Set<ApiMessage>().Add(apiMessage);
        }
    }
}
