using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Company.Queries.GetCompanies;

public class GetCompaniesResponse(IPagedList<Domain.CompanyAggregate.Company> items)
    : PagedListDto<Domain.CompanyAggregate.Company, GetCompanyDto>(items)
{
    public override List<GetCompanyDto> ConvertTToTDto(List<Domain.CompanyAggregate.Company> items)
    {
        List<GetCompanyDto> result = [];

        foreach (Domain.CompanyAggregate.Company company in items)
        {
            result.Add(new GetCompanyDto(
                CompanyId: company.Id.Value,
                Name: company.Name,
                CreatedAt: company.CreatedAt
            ));
        }
        return result;
    }
}

public record GetCompanyDto(
    Guid CompanyId,
    string Name,
    DateTime CreatedAt
);