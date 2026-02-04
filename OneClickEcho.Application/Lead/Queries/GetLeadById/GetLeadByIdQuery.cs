using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Lead.Queries.GetLeadById;

public sealed record GetLeadByIdQuery(Guid LeadId) : IQuery<GetLeadByIdResponse>;