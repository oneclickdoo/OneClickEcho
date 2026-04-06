using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.CampaignAggregate.Repositories;

public interface ICampaignRepository : IRepository<Campaign, CampaignId>
{
    Task<Campaign?> GetByIdAsync(CampaignId id, CancellationToken cancellationToken = default);

    Task<int> GetCountByCompanyId(CompanyId companyId, CancellationToken cancellationToken = default);

    Task<CampaignLead?> RemoveLeadCampaign(CampaignId campaignId, LeadId leadId, CancellationToken cancellationToken = default);

    Task<IPagedList<Campaign>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default);

    Task<List<Campaign>> GetAllByCompanyId(CompanyId companyId, CancellationToken cancellationToken = default);

    Task<List<Campaign>> GetImmediateQueuedCampaigns(CancellationToken cancellationToken = default);

    Task<List<Campaign>> GetScheduledCampaignsByStartDate(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically sets status to <see cref="CampaignStatus.InProgress"/> only if currently <see cref="CampaignStatus.Queued"/>.
    /// Use so duplicate Quartz triggers or multiple app instances cannot run the same send twice.
    /// </summary>
    Task<bool> TryMarkAsInProgressFromQueuedAsync(CampaignId id, CancellationToken cancellationToken = default);

    /// <summary>Viber campaigns still <c>InProgress</c> (any launch time).</summary>
    Task<List<Campaign>> GetInProgressViberCampaignsAsync(CancellationToken cancellationToken = default);
    
    Task<List<Campaign>> GetLast49HoursViberCampaigns(CancellationToken cancellationToken = default);
    
    Task<List<Campaign>> GetLast49HoursViberTwoWayCampaigns(CancellationToken cancellationToken = default);
    
    Task<List<Campaign>> GetExpiredViberCampaigns(CancellationToken cancellationToken = default);

    Task UnassignLeadCollectionFromCampaign(CampaignId campaignId, LeadCollectionId leadCollectionId,
        CancellationToken cancellationToken = default);

    Task AssignLeadCollectionToCampaign(CampaignLeadCollection campaignLeadCollection, CancellationToken cancellationToken = default);

    void Add(Campaign campaign);

    void Delete(Campaign campaign);
}