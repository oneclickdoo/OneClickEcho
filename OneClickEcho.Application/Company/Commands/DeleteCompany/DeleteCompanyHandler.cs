using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Commands.DeleteCompany;

public class DeleteCompanyHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork,
    ICampaignRepository campaignRepository) : ICommandHandler<DeleteCompanyCommand, DeleteCompanyResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<DeleteCompanyResponse>> Handle(DeleteCompanyCommand command,
        CancellationToken cancellationToken)
    {
        Domain.CompanyAggregate.Company? company = await _companyRepository
            .GetByIdAsync(CompanyId.Create(command.Id), cancellationToken);

        if (company is null)
        {
            return Result.Failure<DeleteCompanyResponse>(new Error("NotFound", "Company not found"));
        }

        int campaignCount = await _campaignRepository.GetCountByCompanyId(company.Id, cancellationToken);

        if (campaignCount > 0)
        {
            return Result.Failure<DeleteCompanyResponse>(new Error("HasCampaigns", "Company has campaigns"));
        }

        _companyRepository.Delete(company);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteCompanyResponse());
    }
}

