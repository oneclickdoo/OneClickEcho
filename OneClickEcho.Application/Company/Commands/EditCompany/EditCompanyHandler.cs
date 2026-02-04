using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Commands.EditCompany;

public class EditCompanyHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditCompanyCommand, EditCompanyResponse>
{
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<EditCompanyResponse>> Handle(EditCompanyCommand command,
        CancellationToken cancellationToken)
    {
        // get company
        Domain.CompanyAggregate.Company? company = await _companyRepository
            .GetByIdAsync(CompanyId.Create(command.CompanyId), cancellationToken);

        if (company is null)
        {
            return Result.Failure<EditCompanyResponse>(Error.NullValue);
        }

        company.Name = command.Name;
        company.SmsUsername = command.SmsUsername;
        company.SmsPassword = command.SmsPassword;
        company.ViberPricePerMesssage = command.ViberPricePerMesssage;
        company.SmsPricePerMesssage = command.SmsPricePerMesssage;
        company.ApiPassword = command.ApiPassword;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EditCompanyResponse(company.Id.Value);
    }
}