using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateAndAssignLeadToCollection
{
    public class CreateAndAssignLeadToCollectionHandler(ILeadCollectionRepository leadCollectionRepository,
        ILeadRepository leadRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<CreateAndAssignLeadToCollectionCommand, CreateAndAssignLeadToCollectionResponse>
    {
        private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
        private readonly ILeadRepository _leadRepository = leadRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<CreateAndAssignLeadToCollectionResponse>> Handle(CreateAndAssignLeadToCollectionCommand request, CancellationToken cancellationToken)
        {
            // get lead coollection
            Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
                .GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

            if (leadCollection is null)
            {
                return Result.Failure<CreateAndAssignLeadToCollectionResponse>(new Error(
                    "LeadCollection.NotFound",
                    "Lead Collection not found."
                ));
            }

            // create or update lead
            Domain.LeadAggregate.Lead lead = await LeadHelper.CreateOrUpdate(new Lead.Commands.CreateLead.CreateLeadCommand(
                request.CompanyId,
                null,
                request.PhoneNumber,
                request.FirstName,
                request.LastName,
                request.Gender,
                request.Email,
                request.DateOfBirth,
                request.City,
                request.State,
                request.Country),
                leadCollection.CompanyId,
                _leadRepository);

            // assign lead
            List<LeadId> leadIds = [lead.Id];

            leadCollection.AddLeadAssignments(leadIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateAndAssignLeadToCollectionResponse(lead.Id.Value);
        }
    }
}
