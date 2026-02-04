using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Commands.AssignLeadsToCollection;

public class AssignLeadsToCollectionHandler(ILeadCollectionRepository leadCollectionRepository,
    IUnitOfWork unitOfWork, ILeadRepository leadRepository) : ICommandHandler<AssignLeadsToCollectionCommand,
        AssignLeadsToCollectionResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<AssignLeadsToCollectionResponse>> Handle(AssignLeadsToCollectionCommand request, CancellationToken cancellationToken)
    {
        // get lead coollection
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

        if (leadCollection is null)
        {
            return Result.Failure<AssignLeadsToCollectionResponse>(new Error(
                "LeadCollection.NotFound",
                "Lead Collection not found."
            ));
        }

        // assign leads
        List<LeadId> leadIds = [];

        foreach (LeadId? leadId in request.Leads.Select(leadDto => LeadId.Create(leadDto.LeadId)))
        {
            Domain.LeadAggregate.Lead? lead = await _leadRepository
                .GetByIdAsync(leadId, cancellationToken);

            if (lead is not null)
            {
                leadIds.Add(lead.Id);
            }
        }

        int leadsAdded = leadCollection.AddLeadAssignments(leadIds);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignLeadsToCollectionResponse(leadCollection.Id.Value, leadsAdded);
    }
}