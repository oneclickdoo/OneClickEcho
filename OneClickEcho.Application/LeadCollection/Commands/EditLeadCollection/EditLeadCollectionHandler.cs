using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Commands.EditLeadCollection;

public class EditLeadCollectionHandler(ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditLeadCollectionCommand, EditLeadCollectionResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<EditLeadCollectionResponse>> Handle(EditLeadCollectionCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

        if (leadCollection is null)
        {
            return Result.Failure<EditLeadCollectionResponse>(new Error(
                "LeadCollection.NotFound",
                $"The LeadCollection with Id:\"{request.LeadCollectionId}\" does not exist."
            ));
        }

        leadCollection.CollectionName = request.CollectionName;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EditLeadCollectionResponse(leadCollection.Id.Value);
    }
}