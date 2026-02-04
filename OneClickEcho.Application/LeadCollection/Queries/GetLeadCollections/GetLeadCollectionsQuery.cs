using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.LeadCollection.Queries.GetLeadCollections;

public class GetLeadCollectionsQuery : BasePagedQuery, IQuery<GetLeadCollectionsResponse>
{
    public string? SearchString { get; set; }
}