using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;

namespace OneClickEcho.Application.Company.Queries.GetCompanies;

public class GetCompaniesHandler(ICompanyRepository companyRepository) : IQueryHandler<GetCompaniesQuery, GetCompaniesResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<Result<GetCompaniesResponse>> Handle(GetCompaniesQuery query,
        CancellationToken cancellationToken)
    {
        Domain.Common.Queries.IPagedList<Domain.CompanyAggregate.Company> companies = await _companyRepository
            .GetPagedAsync(query, cancellationToken);

        return new GetCompaniesResponse(companies);
    }
}