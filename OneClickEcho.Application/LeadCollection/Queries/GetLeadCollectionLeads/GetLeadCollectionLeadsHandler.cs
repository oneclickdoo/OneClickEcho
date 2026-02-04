using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionLeads
{
    public class GetLeadCollectionLeadsHandler(ILeadCollectionRepository leadCollectionRepository)
        : IQueryHandler<GetLeadCollectionLeadsQuery, GetLeadCollectionLeadsResponse>
    {
        private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;

        public async Task<Result<GetLeadCollectionLeadsResponse>> Handle(GetLeadCollectionLeadsQuery query, CancellationToken cancellationToken)
        {
            // get collection
            Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
                .GetByIdNoIncludeAsync(LeadCollectionId.Create(query.LeadCollectionId), cancellationToken);
            
            if (leadCollection is null)
            {
                return Result.Failure<GetLeadCollectionLeadsResponse>(new Error(
                    "LeadCollection.NotFound",
                    $"The LeadCollection with Id:\"{query.LeadCollectionId}\" does not exist."
                    ));
            }

            // get collections leads
            Domain.Common.Queries.IPagedList<Domain.LeadAggregate.Lead> campaignLeads = await _leadCollectionRepository
                .GetPagedLeadsById(LeadCollectionId.Create(query.LeadCollectionId), query, cancellationToken);

            return new GetLeadCollectionLeadsResponse(campaignLeads);
        }
    }
}
