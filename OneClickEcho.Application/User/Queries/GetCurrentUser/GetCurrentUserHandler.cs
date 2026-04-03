using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.User.Queries.GetCurrentUser;

public class GetCurrentUserHandler(ICompanyRepository companyRepository)
    : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<Result<GetCurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // @TODO: Fix this shit pls (Get all companies at once)
        List<GetCompanyDto> companies = [];

        foreach (Guid companyId in request.CompanyIds)
        {
            Domain.CompanyAggregate.Company? company = await _companyRepository.GetByIdAsync(CompanyId.Create(companyId), cancellationToken);

            if (company is not null)
            {
                companies.Add(new GetCompanyDto(
                    company.Id.Value,
                    company.Name,
                    company.CreatedAt
                ));
            }
        }

        return new GetCurrentUserResponse(
            request.Email,
            request.Roles,
            companies
        );
    }
}