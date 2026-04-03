using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

public interface ILeadCollectionRepository : IRepository<LeadCollection, LeadCollectionId>
{
    Task<LeadCollection?> GetByIdAsync(LeadCollectionId id, CancellationToken cancellationToken = default);
    
    Task<LeadCollection?> GetByIdNoIncludeAsync(LeadCollectionId id, CancellationToken cancellationToken = default);

    Task<IPagedList<LeadCollection>> GetAllAsync(IPagedQuery query, string? searchString,
        CancellationToken cancellationToken = default);

    Task<List<LeadCollection>> GetAllByCampaignIdAsync(CampaignId campaignId,
        CancellationToken cancellationToken = default);

    Task<IPagedList<LeadCollection>> GetAllByCampaignIdAsync(CampaignId campaignId, IPagedQuery query,
        CancellationToken cancellationToken = default);

    Task<List<LeadCollection>> SearchByNameAsync(string name, CampaignId campaignId,
        CancellationToken cancellationToken = default);

    Task<IPagedList<Lead>> GetPagedLeadsById(LeadCollectionId leadCollectionId, IPagedQuery query,
        CancellationToken cancellationToken = default);
    
    Task<List<LeadCollectionCountDto>> GetLeadCollectionCounts(LeadCollectionId[] leadCollectionId, CancellationToken cancellationToken = default);

    void Add(LeadCollection leadCollection);

    void Remove(LeadCollection leadCollection);
}

public record LeadCollectionCountDto(
    Guid LeadCollectionId,
    int Count
);
 