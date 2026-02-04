using OneClickEcho.Domain.CompanyAggregate.Entities;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.CompanyAggregate.Repositories
{
    public interface ISenderRepository
    {
        Task<Sender?> GetByIdAsync(SenderId id, CancellationToken cancellationToken = default);

        Task<List<Sender>> GetAllByCompanyIdAsync(CompanyId companyId, CancellationToken cancellationToken = default);

        void Add(Sender sender);

        void Delete(Sender sender);
    }
}
