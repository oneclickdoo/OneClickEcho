using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.LeadCollection.Commands.CreateLeadCollection;

public class CreateLeadCollectionHandler(ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateLeadCollectionCommand, CreateLeadCollectionResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateLeadCollectionResponse>> Handle(CreateLeadCollectionCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadCollectionAggregate.LeadCollection leadCollection = new(
                collectionName: request.CollectionName,
                companyId: CompanyId.Create(request.CompanyId)
            );

        _leadCollectionRepository.Add(leadCollection);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLeadCollectionResponse(leadCollection.Id.Value);
    }
}