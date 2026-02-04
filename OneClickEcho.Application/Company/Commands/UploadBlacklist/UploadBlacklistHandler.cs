using System.Text.RegularExpressions;
using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Application.Company.Commands.UploadBlacklist
{
    public class UploadBlacklistHandler(
        ICompanyRepository companyRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICsvReaderService csvReaderService)
        : ICommandHandler<UploadBlacklistCommand, UploadBlacklistResponse>
    {
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ICsvReaderService _csvReaderService = csvReaderService;
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly ILeadRepository _leadRepository = leadRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<UploadBlacklistResponse>> Handle(UploadBlacklistCommand request, CancellationToken cancellationToken)
        {
            var company = await _companyRepository.GetByIdAsync(CompanyId.Create(request.CompanyId), cancellationToken);

            if (company == null)
            {
                return Result.Failure<UploadBlacklistResponse>(new Error(
                    "Company.NotFound",
                    "Company associated with the Lead Collection not found."
                ));
            }

            string filename = await _fileStorageService.SaveFileAsync(request.File, cancellationToken);

            List<CsvLeadDto> records = await _csvReaderService.CsvToLeads(filename, cancellationToken);

            List<LeadId> leadIds = [];

            foreach (CsvLeadDto record in records)
            {
                // if phone number is not valid, ignore record
                if (!Regex.Match(record.PhoneNumber, RegexHelper.PHONE_NUMBER_REGEX).Success)
                {
                    continue;
                }

                Domain.LeadAggregate.Lead lead = await LeadHelper.CreateOrUpdateAndBlacklist(
                    new(
                        company.Id.Value,
                        null,
                        record.PhoneNumber,
                        record.FirstName,
                        record.LastName,
                        record.Gender,
                        record.Email,
                        record.DateOfBirth,
                        record.City,
                        record.State,
                        record.Country),
                    company.Id,
                    _leadRepository);

                leadIds.Add(lead.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UploadBlacklistResponse(company.Id.Value, leadIds.Count);
        }
    }
}
