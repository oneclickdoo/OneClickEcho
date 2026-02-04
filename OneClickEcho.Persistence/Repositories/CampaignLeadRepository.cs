using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Models;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class CampaignLeadRepository(ApplicationDbContext dbContext) : ICampaignLeadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<CampaignLead?> GetByIdAsync(CampaignLeadId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CampaignLead>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<CampaignLead?> GetByCampaignAndLeadId(CampaignId campaignId, LeadId leadId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CampaignLead>()
            .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.LeadId == leadId, cancellationToken);
    }

    public Task<List<Lead>> GetAllLeadsByCampaignIdAsync(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        IQueryable<Lead> leads = from cl in _dbContext.Set<CampaignLead>()
                                 join c in _dbContext.Campaigns on cl.CampaignId equals c.Id
                                 join l in _dbContext.Leads on cl.LeadId equals l.Id
                                 where c.Id == campaignId
                                 select l;

        return leads.ToListAsync(cancellationToken);
    }

    public async Task<IPagedList<Lead>> GetPagedLeadsByCampaignIdAsync(CampaignId campaignId, IPagedQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Lead> leadsQuery = from cl in _dbContext.Set<CampaignLead>()
                                      join c in _dbContext.Campaigns on cl.CampaignId equals c.Id
                                      join l in _dbContext.Leads on cl.LeadId equals l.Id
                                      where c.Id == campaignId
                                      select l;

        PagedList<Lead> leads = await PagedList<Lead>.CreateAsync(leadsQuery, query, cancellationToken);

        return leads;
    }

    public Task<List<CampaignLead>> GetAllCampaignLeadsAsync(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        IQueryable<CampaignLead> campaignLeads = from cl in _dbContext.Set<CampaignLead>()
                                                 where cl.CampaignId == campaignId
                                                 select cl;

        return campaignLeads.ToListAsync(cancellationToken);
    }
    
    public Task<List<CampaignLead>> GetPendingCampaignLeadsAsync(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        IQueryable<CampaignLead> campaignLeads = from cl in _dbContext.Set<CampaignLead>()
            where cl.CampaignId == campaignId
            where cl.ViberStatus == CampaignLeadViberStatus.Pending
            select cl;

        return campaignLeads.ToListAsync(cancellationToken);
    }

    public async Task<List<CampaignLead>> GetNonTerminalCampaignLeadsForCampaignIdsAsync(List<CampaignId> campaignIds, CancellationToken cancellationToken = default)
    {
        //List<CampaignLead> campaignLeads = await _dbContext.CampaignLeads
        //    .Where(x => campaignIds.Contains(x.CampaignId))
        //    .Where(x => x.ViberMessageId > 0)
        //    .Where(x =>
        //        x.ViberStatus != CampaignLeadViberStatus.Expired &&
        //        x.ViberStatus != CampaignLeadViberStatus.Undelivered &&
        //        x.ViberStatus != CampaignLeadViberStatus.Clicked &&
        //        x.ViberStatus != CampaignLeadViberStatus.Delivered &&
        //        x.ViberStatus != CampaignLeadViberStatus.Seen)
        //    .ToListAsync(cancellationToken);

        //return campaignLeads;
        var terminalStatuses = new[]
        {
            CampaignLeadViberStatus.Expired,
            CampaignLeadViberStatus.Undelivered,
            CampaignLeadViberStatus.Clicked,
            CampaignLeadViberStatus.Delivered,
            CampaignLeadViberStatus.Seen
        };

        return await _dbContext.CampaignLeads
            .Where(x => campaignIds.Contains(x.CampaignId))
            .Where(x => x.ViberMessageId > 0)
            .Where(x => !terminalStatuses.Contains(x.ViberStatus)) // uključuje None=0 + Received + Pending
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CampaignLead>> GetAnswerableCampaignLeadsForCampaignIdsAsync(List<CampaignId> campaignIds, CancellationToken cancellationToken = default)
    {
        List<CampaignLead> campaignLeads = await _dbContext.CampaignLeads
            .Where(x => campaignIds.Contains(x.CampaignId))
            .Where(x =>
                x.ViberStatus == CampaignLeadViberStatus.Delivered || 
                x.ViberStatus == CampaignLeadViberStatus.Seen ||
                x.ViberStatus == CampaignLeadViberStatus.Clicked)
            .ToListAsync(cancellationToken);

        return campaignLeads;
    }

    public Task<int> CountUnsubscribedLeadsForCampaignId(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        IQueryable<CampaignLead> campaignLeads = from cl in _dbContext.Set<CampaignLead>()
            join l in _dbContext.Leads on cl.LeadId equals l.Id 
            where cl.CampaignId == campaignId
            where l.IsUnsubscribed == true
            select cl;

        return campaignLeads.CountAsync(cancellationToken);
    }

    public Task<List<Lead>> GetLeadsByCampaignIdAndStatusAsync(Campaign campaign, CampaignLeadViberStatus? campaignLeadViberStatus,
        CampaignLeadSMSStatus? campaignLeadSmsStatus, CancellationToken cancellationToken = default)
    {
        if (campaign.IsSms)
        {
            return (
                from cl in _dbContext.Set<CampaignLead>()
                join l in _dbContext.Leads on cl.LeadId equals l.Id
                where cl.CampaignId == campaign.Id
                where cl.SMSStatus == campaignLeadSmsStatus
                select l
            ).ToListAsync(cancellationToken);
        }

        return (
            from cl in _dbContext.Set<CampaignLead>()
            join l in _dbContext.Leads on cl.LeadId equals l.Id
            where cl.CampaignId == campaign.Id
            where cl.ViberStatus == campaignLeadViberStatus
            select l
        ).ToListAsync(cancellationToken);
    }

    public async Task<CountCampaignLeadsByStatusDto> CountCampaignLeadsByStatus(Campaign campaign, CancellationToken cancellationToken = default)
    {
        if (campaign.IsSms)
        {
            return new CountCampaignLeadsByStatusDto
            {
                PendingCount = await _dbContext.Set<CampaignLead>()
                    .CountAsync(x => x.SMSStatus == CampaignLeadSMSStatus.Pending, cancellationToken),
                DeliveredCount = await _dbContext.Set<CampaignLead>()
                    .CountAsync(x => x.SMSStatus == CampaignLeadSMSStatus.Delivered, cancellationToken),
                UndeliveredCount = await _dbContext.Set<CampaignLead>()
                    .CountAsync(x => x.SMSStatus == CampaignLeadSMSStatus.Undelivired, cancellationToken)
            };
        }

        // TODO: How to handle SMS fallback?

        return new CountCampaignLeadsByStatusDto
        {
            PendingCount = await _dbContext.Set<CampaignLead>()
                .CountAsync(x => x.ViberStatus == CampaignLeadViberStatus.Pending, cancellationToken),
            DeliveredCount = await _dbContext.Set<CampaignLead>()
                .CountAsync(x => x.ViberStatus == CampaignLeadViberStatus.Delivered, cancellationToken),
            UndeliveredCount = await _dbContext.Set<CampaignLead>()
                .CountAsync(x => x.ViberStatus == CampaignLeadViberStatus.Undelivered, cancellationToken)
        };
    }

    public Task<List<LeadWithCampaignLeadDto>> ExportCampaignLeads(CampaignId campaignId,
        CampaignLeadSMSStatus? smsStatus, CampaignLeadViberStatus? viberStatus, CancellationToken cancellationToken = default)
    {
        IQueryable<LeadWithCampaignLeadDto> campaignLeadsResponse = from cl in _dbContext.Set<CampaignLead>()
                                                                    join c in _dbContext.Campaigns on cl.CampaignId equals c.Id
                                                                    join l in _dbContext.Leads on cl.LeadId equals l.Id
                                                                    where c.Id == campaignId
                                                                    where smsStatus != null || cl.SMSStatus == smsStatus
                                                                    where viberStatus != null || cl.ViberStatus == viberStatus
                                                                    select new LeadWithCampaignLeadDto
                                                                    {
                                                                        Lead = l,
                                                                        CampaignLead = cl
                                                                    };

        return campaignLeadsResponse.ToListAsync(cancellationToken);
    }

    public Task<List<MessageSendingCampaignLeadDto>> GetAllLeadsForDateOfBirthAsync(DateOnly dateOfBirth, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS8629 // Nullable value type may be null.
        IQueryable<MessageSendingCampaignLeadDto> leads = from cl in _dbContext.Set<CampaignLead>()
                                                          join c in _dbContext.Campaigns on cl.CampaignId equals c.Id
                                                          join l in _dbContext.Leads on cl.LeadId equals l.Id
                                                          where c.Status == CampaignStatus.InProgress
                                                          where l.DateOfBirth.Value.Day == dateOfBirth.Day
                                                          where l.DateOfBirth.Value.Month == dateOfBirth.Month
                                                          group l by new { c.Id } into g
                                                          select new MessageSendingCampaignLeadDto
                                                          {
                                                              CampaignId = g.Key.Id,
                                                              Leads = g.ToList()
                                                          };
#pragma warning restore CS8629 // Nullable value type may be null.

        return leads.ToListAsync(cancellationToken);
    }

    public void Add(CampaignLead campaignLead)
    {
        // get the highest existing counter (ViberMessageId) value (or set to 1 if no leads exist)
        // long lastCounter = _dbContext.CampaignLeads.Any() ? _dbContext.CampaignLeads.Max(x => x.ViberMessageId) : 1;
        //
        // // set the counter to the next value
        // campaignLead.ViberMessageId = lastCounter + 1;

        _dbContext.Set<CampaignLead>().Add(campaignLead);
    }
    
    public void Delete(CampaignLead campaignLead)
    {
        _dbContext.Set<CampaignLead>().Remove(campaignLead);
    }

    public Task AddReceivedMessages(List<ReceivedMessage> receivedMessages)
    {
        return _dbContext.ReceivedMessages.AddRangeAsync(receivedMessages);
    }
}