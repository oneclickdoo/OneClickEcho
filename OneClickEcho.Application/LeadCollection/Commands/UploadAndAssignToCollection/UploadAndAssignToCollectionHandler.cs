using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.LeadCollection.Commands.UploadAndAssignToCollection;

public class UploadAndAssignLeadsToCollectionHandler(
    ILeadCollectionRepository leadCollectionRepository,
    IUnitOfWork unitOfWork,
    ILeadRepository leadRepository,
    IFileStorageService fileStorageService,
    ICsvReaderService csvReaderService) : ICommandHandler<UploadAndAssignLeadsToCollectionCommand,
        UploadAndAssignLeadsToCollectionResponse>
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ICsvReaderService _csvReaderService = csvReaderService;
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<UploadAndAssignLeadsToCollectionResponse>> Handle(UploadAndAssignLeadsToCollectionCommand request, CancellationToken cancellationToken)
    {
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

        if (leadCollection is null)
        {
            return Result.Failure<UploadAndAssignLeadsToCollectionResponse>(new Error(
                "LeadCollection.NotFound",
                "Lead Collection not found."
            ));
        }

        string filename = await _fileStorageService.SaveFileAsync(request.File, cancellationToken);

        List<CsvLeadDto> records = await _csvReaderService.CsvToLeads(filename, cancellationToken);

        List<LeadId> leadIds = [];

        HashSet<string> csvNormalizedKeys = records
            .Select(r => PhoneNumberHelper.NormalizeKey(r.PhoneNumber))
            .Where(k => !string.IsNullOrEmpty(k))
            .ToHashSet(StringComparer.Ordinal);

        List<Domain.LeadAggregate.Lead> duplicateLeads =
            await _leadRepository.GetLeadsByCompanyMatchingNormalizedPhonesAsync(
                CompanyId.Create(leadCollection.CompanyId.Value),
                csvNormalizedKeys,
                cancellationToken);

        HashSet<string> existingNormalizedKeys = duplicateLeads
            .Select(l => PhoneNumberHelper.NormalizeKey(l.PhoneNumber))
            .Where(k => !string.IsNullOrEmpty(k))
            .ToHashSet(StringComparer.Ordinal);

        List<Domain.LeadAggregate.Lead> newRecords = records
            .Where(l => !string.IsNullOrEmpty(PhoneNumberHelper.NormalizeKey(l.PhoneNumber)))
            .Where(l => !existingNormalizedKeys.Contains(PhoneNumberHelper.NormalizeKey(l.PhoneNumber)))
            .GroupBy(l => PhoneNumberHelper.NormalizeKey(l.PhoneNumber))
            .Select(g => g.First())
            .Select(l => new Domain.LeadAggregate.Lead(
                companyId: CompanyId.Create(leadCollection.CompanyId.Value),
                phoneNumber: PhoneNumberHelper.Standardize(l.PhoneNumber),
                firstName: l.FirstName,
                lastName: l.LastName,
                gender: l.Gender,
                email: l.Email,
                dateOfBirth: l.DateOfBirth,
                city: l.City,
                state: l.State,
                country: l.Country))
            .ToList();
        
        _leadRepository.AddRange(newRecords);
        
        // We still want to assign old leads to the collection, so we concatenate
        // the duplicates and the new records, those won't have any overlap
        leadIds.AddRange(duplicateLeads.Select(l => l.Id).Concat(newRecords.Select(l => l.Id)));
        
        int leadsAdded = leadCollection.BulkAddLeadAssignments(leadIds);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadAndAssignLeadsToCollectionResponse(leadCollection.Id.Value, leadsAdded);
    }
}