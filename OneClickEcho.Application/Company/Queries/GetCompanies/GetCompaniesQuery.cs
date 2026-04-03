using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Queries.GetCompanies;

public class GetCompaniesQuery : BasePagedQuery, IQuery<GetCompaniesResponse>;