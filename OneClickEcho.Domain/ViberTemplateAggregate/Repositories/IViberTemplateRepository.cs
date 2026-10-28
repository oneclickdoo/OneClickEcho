using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.ViberTemplateAggregate.ValueObjects;

namespace OneClickEcho.Domain.ViberTemplateAggregate.Repositories;

public interface IViberTemplateRepository : IRepository<ViberTemplate, ViberTemplateId>
{
    Task<ViberTemplate?> GetByIdAsync(ViberTemplateId id, CancellationToken cancellationToken = default);

    Task<List<ViberTemplate>> ListByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default);

    void Add(ViberTemplate template);

    void Remove(ViberTemplate template);
}
