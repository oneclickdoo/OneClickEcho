using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.NounCaseAggregate.ValueObjects;

namespace OneClickEcho.Domain.NounCaseAggregate.Repositories;

public interface INounCaseRepository : IRepository<NounCase, NounCaseId>
{
    Task<NounCase?> GetByNominativeAsync(string nominative, CancellationToken cancellationToken = default);
    void Add(NounCase nounCase);
}