using Microsoft.EntityFrameworkCore;
using OneClickEcho.Domain.Common;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Entities;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Persistence.Common;

namespace OneClickEcho.Persistence.Repositories;

public class LeadRepository(ApplicationDbContext dbContext) : ILeadRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Lead?> GetByIdAsync(LeadId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Lead?> GetByPhoneNumberAsync(string phoneNumber, CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .FirstOrDefaultAsync(g => g.PhoneNumber == phoneNumber && g.CompanyId == companyId, cancellationToken);
    }

    public async Task<List<Lead>> GetAllByPhoneNumbersAndCompanyIdAsync(List<string> phoneNumbers, CompanyId companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .Where(x => x.CompanyId == companyId)
            .Where(x => phoneNumbers.Contains(x.PhoneNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetLeadsByCompanyMatchingNormalizedPhonesAsync(
        CompanyId companyId,
        IReadOnlyCollection<string> normalizedPhoneKeys,
        CancellationToken cancellationToken = default)
    {
        if (normalizedPhoneKeys.Count == 0)
        {
            return [];
        }

        HashSet<string> keySet = normalizedPhoneKeys is HashSet<string> hs
            ? hs
            : new HashSet<string>(normalizedPhoneKeys, StringComparer.Ordinal);

        List<Lead> leads = await _dbContext.Set<Lead>()
            .Where(x => x.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return leads
            .Where(l => keySet.Contains(PhoneNumberHelper.NormalizeKey(l.PhoneNumber)))
            .ToList();
    }

    public async Task<IPagedList<Lead>> GetAllAsync(IPagedQuery query, CancellationToken cancellationToken = default)
    {
        PagedList<Lead> pagedList = await PagedList<Lead>
            .CreateAsync(_dbContext.Leads, query, cancellationToken);

        return pagedList;
    }

    public async Task<List<Lead>> GetAllByCompanyId(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .Where(x => x.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetAllInLeadIdList(CompanyId companyId, List<LeadId> leadIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .Where(x => x.CompanyId == companyId && leadIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Lead>> GetAllByCollectionIdAsync(LeadCollectionId leadCollectionId, CancellationToken cancellationToken = default)
    {
        var data = await _dbContext
            .Set<Lead>()
            .Join(_dbContext.Set<LeadAssignment>(),
                l => l.Id,
                la => la.LeadId,
                (l, la) => new { Lead = l, LeadAssignment = la })
            .Where(x => x.LeadAssignment.LeadCollectionId == leadCollectionId)
            .Select(x => x.Lead)
            .ToListAsync(cancellationToken);
        
        return data;
    }

    public void Add(Lead lead)
    {
        _dbContext.Set<Lead>().Add(lead);
    }

    public void AddRange(List<Lead> leads)
    {
        _dbContext.Set<Lead>().AddRange(leads);
    }

    public void Delete(Lead lead)
    {
        _dbContext.Set<Lead>().Remove(lead);
    }
}