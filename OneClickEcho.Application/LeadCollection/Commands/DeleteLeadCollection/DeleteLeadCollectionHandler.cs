using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Commands.DeleteLeadCollection;

public class DeleteLeadCollectionHandler(ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteLeadCollectionCommand, DeleteLeadCollectionResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<DeleteLeadCollectionResponse>> Handle(DeleteLeadCollectionCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdNoIncludeAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

        if (leadCollection is null)
        {
            return Result.Failure<DeleteLeadCollectionResponse>(new Error(
                "LeadCollection.NotFound",
                $"The LeadCollection with Id:\"{request.LeadCollectionId}\" does not exist."
            ));
        }

        _leadCollectionRepository.Remove(leadCollection);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteLeadCollectionResponse(leadCollection.Id.Value);
    }
}