using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Application.Lead.Commands.EditLead;

public class EditLeadHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditLeadCommand, EditLeadResponse>
{
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<EditLeadResponse>> Handle(EditLeadCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadAggregate.Lead? lead = await _leadRepository
            .GetByIdAsync(LeadId.Create(request.LeadId), cancellationToken);

        if (lead is null)
        {
            return Result.Failure<EditLeadResponse>(new Error(
                "Lead.NotFound",
                $"The Lead with Id:\"{request.LeadId}\" does not exist."
            ));
        }

        lead.PhoneNumber = request.PhoneNumber;
        lead.Email = request.Email;
        lead.FirstName = request.FirstName;
        lead.LastName = request.LastName;
        lead.Gender = request.Gender;
        lead.DateOfBirth = request.DateOfBirth;
        lead.City = request.City;
        lead.State = request.State;
        lead.Country = request.Country;
        lead.IsBlacklisted = request.IsBlacklisted ?? false;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EditLeadResponse(lead.Id.Value);
    }
}