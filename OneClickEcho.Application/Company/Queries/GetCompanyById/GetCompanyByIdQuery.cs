using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Queries.GetCompanyById;

public record GetCompanyByIdQuery(Guid CompanyId) : IQuery<GetCompanyByIdResponse>;