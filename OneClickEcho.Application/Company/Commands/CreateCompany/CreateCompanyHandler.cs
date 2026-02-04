using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;

namespace OneClickEcho.Application.Company.Commands.CreateCompany;

public class CreateCompanyHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCompanyCommand, CreateCompanyResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateCompanyResponse>> Handle(CreateCompanyCommand command,
        CancellationToken cancellationToken)
    {
        Domain.CompanyAggregate.Company company = new(command.Name);

        _companyRepository.Add(company);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCompanyResponse(company.Id.Value);
    }
}
