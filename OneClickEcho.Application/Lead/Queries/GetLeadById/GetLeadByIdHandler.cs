using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Application.Lead.Queries.GetLeadById;

public class GetLeadByIdHandler(ILeadRepository leadRepository) : IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse>
{
    private readonly ILeadRepository _leadRepository = leadRepository;

    public async Task<Result<GetLeadByIdResponse>> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.LeadAggregate.Lead? lead = await _leadRepository.GetByIdAsync(LeadId.Create(request.LeadId), cancellationToken);
        return lead is null
            ? Result.Failure<GetLeadByIdResponse>(new Error(
                "Lead.NotFound",
                $"The Lead with Id:\"{request.LeadId}\" does not exist."
            ))
            : Result.Success(new GetLeadByIdResponse(
            lead.PhoneNumber,
            lead.FirstName,
            lead.LastName,
            lead.Gender,
            lead.Email,
            lead.DateOfBirth,
            lead.City,
            lead.State,
            lead.Country
        ));
    }
}