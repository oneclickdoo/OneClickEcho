using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollectionLeads
{
    public class GetLeadCollectionLeadsQuery() : BasePagedQuery, IQuery<GetLeadCollectionLeadsResponse>
    {
        public Guid LeadCollectionId;
    };
}
