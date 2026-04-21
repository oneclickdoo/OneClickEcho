using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
using Microsoft.Extensions.Configuration;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class CampaignLeadRepository(ApplicationDbContext dbContext, IConfiguration configuration)
    : ICampaignLeadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Uses <see cref="IConfiguration"/> plus direct process env reads. Some hosts load <c>.env</c> into the process
    /// environment but not into the configuration pipeline; taking the max avoids staying at appsettings.json default 0.
    /// </summary>
    private long ViberMessageIdFloor => ResolveViberMessageIdFloor(_configuration);

    private static long ResolveViberMessageIdFloor(IConfiguration configuration)
    {
        long floor = Math.Max(0, configuration.GetValue<long>("Messaging:CampaignLeadViberMessageId:Floor"));

        static bool TryParseFloor(string? raw, out long value)
        {
            value = 0;
            return !string.IsNullOrWhiteSpace(raw)
                && long.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                && value >= 0;
        }

        if (TryParseFloor(Environment.GetEnvironmentVariable("Messaging__CampaignLeadViberMessageId__Floor"), out long fromNested))
        {
            floor = Math.Max(floor, fromNested);
        }

        if (TryParseFloor(Environment.GetEnvironmentVariable("VIBER_CAMPAIGN_MESSAGE_ID_FLOOR"), out long fromShort))
        {
            floor = Math.Max(floor, fromShort);
        }

        return floor;
    }

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

    public Task<List<Lead>> GetLeadsByCampaignIdWithViberStatusNoneAsync(CampaignId campaignId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<CampaignLead>()
            .Where(cl => cl.CampaignId == campaignId && cl.ViberStatus == CampaignLeadViberStatus.None)
            .Join(_dbContext.Leads, cl => cl.LeadId, l => l.Id, (cl, l) => l)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Lead>> GetLeadsByCampaignIdWithSmsStatusNoneAsync(CampaignId campaignId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<CampaignLead>()
            .Where(cl => cl.CampaignId == campaignId && cl.SMSStatus == CampaignLeadSMSStatus.None)
            .Join(_dbContext.Leads, cl => cl.LeadId, l => l.Id, (cl, l) => l)
            .ToListAsync(cancellationToken);
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

    public async Task<IPagedList<CampaignLeadReportRow>> GetCampaignLeadReportAsync(
        CampaignId campaignId,
        string? phoneSearch,
        CampaignLeadViberStatus? viberStatus,
        CampaignLeadSMSStatus? smsStatus,
        bool? isUnsubscribed,
        IPagedQuery paging,
        CancellationToken cancellationToken = default)
    {
        // Lead report can run COUNT + JOIN + ORDER BY over large campaign sets; default ~30s often surfaces as HTTP 500.
        int reportTimeoutSeconds = _configuration.GetValue("Persistence:CampaignLeadReportCommandTimeoutSeconds", 120);
        if (reportTimeoutSeconds < 30)
        {
            reportTimeoutSeconds = 120;
        }

        int? previousTimeout = _dbContext.Database.GetCommandTimeout();
        try
        {
            _dbContext.Database.SetCommandTimeout(reportTimeoutSeconds);

            IQueryable<CampaignLead> campaignLeadScope = _dbContext.Set<CampaignLead>()
                .Where(cl => cl.CampaignId == campaignId);

            if (viberStatus.HasValue)
            {
                CampaignLeadViberStatus v = viberStatus.Value;
                campaignLeadScope = campaignLeadScope.Where(cl => cl.ViberStatus == v);
            }

            if (smsStatus.HasValue)
            {
                CampaignLeadSMSStatus s = smsStatus.Value;
                campaignLeadScope = campaignLeadScope.Where(cl => cl.SMSStatus == s);
            }

            bool needsLeadForCount =
                !string.IsNullOrWhiteSpace(phoneSearch) || isUnsubscribed.HasValue;

            int totalCount;
            if (needsLeadForCount)
            {
                var qCount = from cl in campaignLeadScope
                    join l in _dbContext.Leads on cl.LeadId equals l.Id
                    select new { cl, l };

                if (!string.IsNullOrWhiteSpace(phoneSearch))
                {
                    string term = phoneSearch.Trim();
                    qCount = qCount.Where(x => x.l.PhoneNumber != null && x.l.PhoneNumber.Contains(term));
                }

                if (isUnsubscribed.HasValue)
                {
                    bool u = isUnsubscribed.Value;
                    qCount = qCount.Where(x => x.l.IsUnsubscribed == u);
                }

                totalCount = await qCount.CountAsync(cancellationToken);
            }
            else
            {
                totalCount = await campaignLeadScope.CountAsync(cancellationToken);
            }

            var qPage = from cl in campaignLeadScope
                join l in _dbContext.Leads on cl.LeadId equals l.Id
                select new { cl, l };

            if (!string.IsNullOrWhiteSpace(phoneSearch))
            {
                string term = phoneSearch.Trim();
                qPage = qPage.Where(x => x.l.PhoneNumber != null && x.l.PhoneNumber.Contains(term));
            }

            if (isUnsubscribed.HasValue)
            {
                bool u = isUnsubscribed.Value;
                qPage = qPage.Where(x => x.l.IsUnsubscribed == u);
            }

            int page = paging.Page;
            int pageSize = paging.PageSize;

            List<CampaignLeadReportRow> items = await qPage
                .OrderBy(x => x.l.PhoneNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CampaignLeadReportRow
                {
                    PhoneNumber = x.l.PhoneNumber ?? string.Empty,
                    ViberStatus = (short)x.cl.ViberStatus,
                    ViberStatusDescription = x.cl.ViberStatusDescription,
                    SmsStatus = (short)x.cl.SMSStatus,
                    SmsStatusDescription = x.cl.SMSStatusDescription,
                    IsUnsubscribed = x.l.IsUnsubscribed
                })
                .ToListAsync(cancellationToken);

            return PagedList<CampaignLeadReportRow>.CreateFromParts(items, page, pageSize, totalCount);
        }
        finally
        {
            _dbContext.Database.SetCommandTimeout(previousTimeout);
        }
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
        // Delivery polling updates all statuses except terminal Clicked and Expired (incl. Undelivered — provider may correct).
        List<CampaignLead> campaignLeads = await _dbContext.CampaignLeads
            .Where(x => campaignIds.Contains(x.CampaignId))
            .Where(x =>
                x.ViberStatus != CampaignLeadViberStatus.Expired &&
                x.ViberStatus != CampaignLeadViberStatus.Clicked)
            .ToListAsync(cancellationToken);

        return campaignLeads;
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

    /// <summary>Serializes global Viber message id assignment with concurrent launches / lead imports.</summary>
    private const int GlobalViberMessageIdAllocationLockKey = 834291117;

    public async Task AssignSequentialGlobalViberMessageIdsAsync(IReadOnlyCollection<CampaignLead> campaignLeads,
        CancellationToken cancellationToken = default)
    {
        if (campaignLeads.Count == 0)
        {
            return;
        }

        await using IDbContextTransaction tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({GlobalViberMessageIdAllocationLockKey})",
            cancellationToken);

        long maxCl = 0;
        if (await _dbContext.Set<CampaignLead>().AnyAsync(cancellationToken))
        {
            maxCl = await _dbContext.Set<CampaignLead>().MaxAsync(x => x.ViberMessageId, cancellationToken);
        }

        long next = Math.Max(maxCl, ViberMessageIdFloor) + 1;
        foreach (CampaignLead cl in campaignLeads)
        {
            cl.ViberMessageId = next++;
        }

        await tx.CommitAsync(cancellationToken);
    }

    public Task SyncCampaignLeadViberMessageIdSequenceAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.ExecuteSqlRawAsync(
            """
            SELECT setval(
                COALESCE(
                    pg_get_serial_sequence('public.campaign_leads', 'viber_message_id'),
                    'public.campaign_leads_viber_message_id_seq'),
                GREATEST(COALESCE((SELECT MAX(viber_message_id) FROM campaign_leads), 0), {0}),
                true);
            """,
            new object[] { ViberMessageIdFloor },
            cancellationToken);
    }
}