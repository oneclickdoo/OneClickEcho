using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionById;

public class GetLeadCollectionByIdHandler(ILeadCollectionRepository leadCollectionRepository)
    : IQueryHandler<GetLeadCollectionByIdQuery, GetLeadCollectionByIdResponse>
{
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

    public async Task<Result<GetLeadCollectionByIdResponse>> Handle(GetLeadCollectionByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdNoIncludeAsync(LeadCollectionId.Create(query.LeadCollectionId), cancellationToken);

        return leadCollection is null
            ? Result.Failure<GetLeadCollectionByIdResponse>(new Error(
                "LeadCollection.NotFound",
                $"The LeadCollection with Id:\"{query.LeadCollectionId}\" does not exist."
            ))
            : new GetLeadCollectionByIdResponse(
                leadCollection.Id.Value,
                leadCollection.CollectionName,
                leadCollection.CreatedAt
        );
    }
}