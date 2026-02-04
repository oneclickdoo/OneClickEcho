using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Queries.GetCompanyImagesById;

public class GetCompanyImagesByIdHandler(ICompanyRepository companyRepository)
    : IQueryHandler<GetCompanyImagesByIdQuery, GetCompanyImagesByIdResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<Result<GetCompanyImagesByIdResponse>> Handle(GetCompanyImagesByIdQuery query, CancellationToken cancellationToken)
    {
        Domain.CompanyAggregate.Company? company = await _companyRepository
            .GetByIdAsync(CompanyId.Create(query.CompanyId), cancellationToken);

        if (company is null)
        {
            return Result.Failure<GetCompanyImagesByIdResponse>(new Error(
                "Company.NotFound",
                $"The Company with Id:\"{query.CompanyId}\" does not exist."
            ));
        }
        
        List<string> images = await _companyRepository.GetCompanyImagesAsync(company.Id, cancellationToken);
        return new GetCompanyImagesByIdResponse(images);
    }
}