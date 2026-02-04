using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.LeadAggregate.Repositories;

public interface ILeadRepository : IRepository<Lead, LeadId>
{
    Task<Lead?> GetByIdAsync(LeadId id, CancellationToken cancellationToken = default);

    Task<Lead?> GetByPhoneNumberAsync(string phoneNumber, CompanyId companyId, CancellationToken cancellationToken = default);
    
    Task<List<Lead>> GetAllByPhoneNumbersAndCompanyIdAsync(List<string> phoneNumber, CompanyId companyId, CancellationToken cancellationToken = default);

    Task<IPagedList<Lead>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default);

    public Task<List<Lead>> GetAllByCompanyId(CompanyId companyId, CancellationToken cancellationToken = default);

    public Task<List<Lead>> GetAllInLeadIdList(CompanyId companyId, List<LeadId> leadIds, CancellationToken cancellationToken = default);

    public Task<List<Lead>> GetAllByCollectionIdAsync(LeadCollectionId leadCollectionId, CancellationToken cancellationToken = default);

    void Add(Lead lead);

    void AddRange(List<Lead> leads);
    
    void Delete(Lead lead);
}