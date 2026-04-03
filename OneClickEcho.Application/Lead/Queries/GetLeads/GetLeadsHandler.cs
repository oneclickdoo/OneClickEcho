using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;

namespace OneClickEcho.Application.Lead.Queries.GetLeads;

public class GetLeadsHandler(ILeadRepository leadRepository) : IQueryHandler<GetLeadsQuery, GetLeadsResponse>
{
    private readonly ILeadRepository _leadRepository = leadRepository;

    public async Task<Result<GetLeadsResponse>> Handle(GetLeadsQuery query, CancellationToken cancellationToken)
    {
        Domain.Common.Queries.IPagedList<Domain.LeadAggregate.Lead> leads = await _leadRepository.GetAllAsync(query, cancellationToken);

        return new GetLeadsResponse(leads);
    }
}