using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Queries.GetCompanyById;

public class GetCompanyByIdHandler(ICompanyRepository companyRepository)
    : IQueryHandler<GetCompanyByIdQuery, GetCompanyByIdResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<Result<GetCompanyByIdResponse>> Handle(GetCompanyByIdQuery query, CancellationToken cancellationToken)
    {
        Domain.CompanyAggregate.Company? company = await _companyRepository
            .GetByIdAsync(CompanyId.Create(query.CompanyId), cancellationToken);

        return company is null
            ? Result.Failure<GetCompanyByIdResponse>(new Error(
                "Company.NotFound",
                $"The Company with Id:\"{query.CompanyId}\" does not exist."
            ))
            : (Result<GetCompanyByIdResponse>)new GetCompanyByIdResponse(
            company.Id.Value,
            company.Name,
            company.SmsUsername,
            company.SmsPassword,
            company.ViberPricePerMesssage,
            company.SmsPricePerMesssage,
            company.ApiPassword,
            company.CreatedAt
        );
    }
}