using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Lead.Queries.GetLeads;

public class GetLeadsQuery : BasePagedQuery, IQuery<GetLeadsResponse>;