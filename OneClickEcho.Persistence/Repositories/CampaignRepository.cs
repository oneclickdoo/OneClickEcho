using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class CampaignRepository(ApplicationDbContext dbContext) : ICampaignRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Campaign?> GetByIdAsync(CampaignId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Campaign>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<int> GetCountByCompanyId(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Campaign>()
            .CountAsync(g => g.CompanyId == companyId, cancellationToken);
    }

    public async Task<CampaignLead?> RemoveLeadCampaign(CampaignId campaignId, LeadId leadId, CancellationToken cancellationToken = default)
    {
        CampaignLead? campaignLead = await _dbContext.Set<CampaignLead>()
            .FirstOrDefaultAsync(g => g.CampaignId == campaignId && g.LeadId == leadId, cancellationToken);

        if (campaignLead is null)
        {
            return null;
        }

        _dbContext.Set<CampaignLead>().Remove(campaignLead);

        return campaignLead;
    }

    public async Task<IPagedList<Campaign>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default)
    {
        PagedList<Campaign> pagedList = await PagedList<Campaign>
            .CreateAsync(_dbContext.Campaigns, query, cancellationToken);

        return pagedList;
    }

    public async Task<List<Campaign>> GetAllByCompanyId(CompanyId companyId, CancellationToken cancellationToken)
    {
        List<Campaign> campaigns = await _dbContext.Campaigns
            .Where(c => c.CompanyId == companyId).ToListAsync(cancellationToken);

        return campaigns;
    }

    public async Task<List<Campaign>> GetImmediateQueuedCampaigns(CancellationToken cancellationToken = default)
    {
        List<Campaign> campaigns = await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.Queued)
            .Where(c => c.SendingType == CampaignSendingType.Immediate)
            .ToListAsync(cancellationToken);

        return campaigns;
    }

    public async Task<List<Campaign>> GetScheduledCampaignsByStartDate(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        List<Campaign> campaigns = await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.Queued)
            .Where(c => c.SendingType == CampaignSendingType.ScheduledDateTime)
            .Where(c => c.SendingDatetime.ToUniversalTime() > startDate.ToUniversalTime()
                        && c.SendingDatetime.ToUniversalTime() <= endDate.ToUniversalTime())
            .ToListAsync(cancellationToken);

        return campaigns;
    }

    public async Task<bool> TryMarkAsInProgressFromQueuedAsync(CampaignId id, CancellationToken cancellationToken = default)
    {
        int updated = await _dbContext.Campaigns
            .Where(c => c.Id == id && c.Status == CampaignStatus.Queued)
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.Status, CampaignStatus.InProgress),
                cancellationToken);
        return updated > 0;
    }

    public async Task<List<Campaign>> GetInProgressViberCampaignsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.InProgress)
            .Where(c => c.IsViber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Viber delivery polling: only campaigns in the last 49 hours (not all campaigns)—by
    /// <c>SendingDatetime</c> or by <c>campaign_leads</c> created in that window with a Viber message id.
    /// Avoids <c>ToUniversalTime()</c> on <c>timestamp without time zone</c> so it matches launch-time dates.
    /// </summary>
    public async Task<List<Campaign>> GetLast49HoursViberCampaigns(CancellationToken cancellationToken = default)
    {
        const int hoursWindow = 49;
        DateTime endDate = DateTime.Now;
        DateTime startDate = endDate.AddHours(-hoursWindow);

        return await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.InProgress || c.Status == CampaignStatus.Done)
            .Where(c => c.IsViber)
            .Where(c =>
                (c.SendingDatetime >= startDate && c.SendingDatetime <= endDate)
                || _dbContext.CampaignLeads.Any(cl =>
                    cl.CampaignId == c.Id
                    && cl.CreatedAt >= startDate
                    && cl.CreatedAt <= endDate
                    && cl.ViberMessageId > 0))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetLast49HoursViberTwoWayCampaigns(CancellationToken cancellationToken = default)
    {
        const int hoursWindow = 49;
        DateTime endDate = DateTime.Now;
        DateTime startDate = endDate.AddHours(-hoursWindow);

        return await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.InProgress || c.Status == CampaignStatus.Done)
            .Where(c => c.IsViber)
            .Where(c => c.IsViberReceivable)
            .Where(c =>
                (c.SendingDatetime >= startDate && c.SendingDatetime <= endDate)
                || _dbContext.CampaignLeads.Any(cl =>
                    cl.CampaignId == c.Id
                    && cl.CreatedAt >= startDate
                    && cl.CreatedAt <= endDate
                    && cl.ViberMessageId > 0))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetExpiredViberCampaigns(CancellationToken cancellationToken = default)
    {
        DateTime date = DateTime.Now.AddHours(-49);
        
        List<Campaign> campaigns = await _dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.Done)
            .Where(c => c.IsViber)
            .Where(c => c.SendingDatetime.ToUniversalTime() < date.ToUniversalTime())
            .ToListAsync(cancellationToken);

        return campaigns;
    }

    public async Task UnassignLeadCollectionFromCampaign(CampaignId campaignId, LeadCollectionId leadCollectionId, CancellationToken cancellationToken = default)
    {
        CampaignLeadCollection? campaignLeadCollection = await _dbContext.Set<CampaignLeadCollection>()
            .Where(x => x.CampaignId == campaignId)
            .Where(x => x.LeadCollectionId == leadCollectionId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Campaign lead collection not found.");

        _dbContext.Set<CampaignLeadCollection>().Remove(campaignLeadCollection);
    }

    public async Task AssignLeadCollectionToCampaign(CampaignLeadCollection campaignLeadCollection, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _dbContext.Set<CampaignLeadCollection>()
                .AnyAsync(clc => clc.CampaignId == campaignLeadCollection.CampaignId &&
                                 clc.LeadCollectionId == campaignLeadCollection.LeadCollectionId, cancellationToken);

            if (!exists)
            {
                _dbContext.Set<CampaignLeadCollection>().Add(campaignLeadCollection);
            }
        }
        catch
        {
            // ignore
        }
    }

    public void Add(Campaign campaign)
    {
        _dbContext.Set<Campaign>().Add(campaign);
    }

    public void Delete(Campaign campaign)
    {
        _dbContext.Set<Campaign>().Remove(campaign);
    }
}