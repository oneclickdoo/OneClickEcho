using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class LeadCollectionRepository(ApplicationDbContext dbContext) : ILeadCollectionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<LeadCollection?> GetByIdAsync(LeadCollectionId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<LeadCollection>()
            .Include(x => x.LeadAssignments)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
    
    public async Task<LeadCollection?> GetByIdNoIncludeAsync(LeadCollectionId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<LeadCollection>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IPagedList<LeadCollection>> GetAllAsync(IPagedQuery query, string? searchString, CancellationToken cancellationToken = default)
    {
        PagedList<LeadCollection> pagedList = await PagedList<LeadCollection>
            .CreateAsync(_dbContext.LeadCollections
                .Where(x => searchString == null || x.CollectionName == searchString), query, cancellationToken);

        return pagedList;
    }
    
    public async Task<List<LeadCollection>> GetAllByCampaignIdAsync(CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        var data = await _dbContext
            .Set<LeadCollection>()
            .Join(_dbContext.Set<CampaignLeadCollection>(),
                lc => lc.Id,
                clc => clc.LeadCollectionId,
                (lc, clc) => new { LeadCollection = lc, CampaignLeadCollection = clc })
            .Where(x => x.CampaignLeadCollection.CampaignId == campaignId)
            .Select(x => x.LeadCollection)
            .ToListAsync(cancellationToken);
        
        return data;
    }

    public async Task<IPagedList<LeadCollection>> GetAllByCampaignIdAsync(CampaignId campaignId, IPagedQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<LeadCollection> campaignLeadCollections = _dbContext.LeadCollections
            .Where(lc =>
                _dbContext.Set<CampaignLeadCollection>()
                    .Where(clc => clc.CampaignId == campaignId)
                    .Select(clc => clc.LeadCollectionId)
                    .Contains(lc.Id)
                );

        PagedList<LeadCollection> pagedList = await PagedList<LeadCollection>
            .CreateAsync(campaignLeadCollections, query, cancellationToken);

        return pagedList;
    }

    public async Task<List<LeadCollection>> SearchByNameAsync(string? name, CampaignId campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _dbContext.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId, cancellationToken);
        if (campaign == null) throw new Exception("Campaign not found.");
        
        List<LeadCollection> leadCollections = await _dbContext.Set<LeadCollection>()
            .Where(lc => (name == null || lc.CollectionName.ToLower().Contains(name.ToLower())) &&
                !_dbContext.Set<CampaignLeadCollection>()
                    .Any(clc => clc.CampaignId == campaignId && clc.LeadCollectionId == lc.Id) &&
                lc.CompanyId == campaign.CompanyId
            )
            .Take(5)
            .ToListAsync(cancellationToken);

        return leadCollections;
    }

    public async Task<IPagedList<Lead>> GetPagedLeadsById(LeadCollectionId id, IPagedQuery query, CancellationToken cancellationToken = default)
    {
        PagedList<Lead> pagedList = await PagedList<Lead>
            .CreateAsync(
                _dbContext.LeadCollections
                    .Where(lc => lc.Id == id)
                    .Join(_dbContext.LeadAssignments,
                        lc => lc.Id,
                        la => la.LeadCollectionId,
                        (lc, la) => la)
                    .Join(_dbContext.Leads,
                        la => la.LeadId,
                        l => l.Id,
                        (la, l) => l),
                query,
                cancellationToken
            );
        
        return pagedList;
    }

    public async Task<List<LeadCollectionCountDto>> GetLeadCollectionCounts(LeadCollectionId[] leadCollectionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<LeadCollection>()
            .Where(lc => leadCollectionId.Contains(lc.Id))
            .Select(lc => new LeadCollectionCountDto(
                lc.Id.Value,
                lc.LeadAssignments.Count
            ))
            .ToListAsync(cancellationToken);
    }


    public void Add(LeadCollection leadCollection)
    {
        _dbContext.Set<LeadCollection>().Add(leadCollection);
    }

    public void Remove(LeadCollection leadCollection)
    {
        _dbContext.Set<LeadCollection>().Remove(leadCollection);
    }
}