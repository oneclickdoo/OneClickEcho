using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using System.Text.RegularExpressions;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Lead.Commands.UploadLeads;

public partial class UploadLeadsHandler(IFileStorageService fileStorageService, ICsvReaderService csvReaderService,
    ILeadRepository leadRepository, ICampaignLeadRepository campaignLeadRepository, ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UploadLeadsCommand, UploadLeadsResponse>
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ICsvReaderService _csvReaderService = csvReaderService;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<UploadLeadsResponse>> Handle(UploadLeadsCommand request, CancellationToken cancellationToken)
    {
        // save file
        string filename = await _fileStorageService.SaveFileAsync(request.File, cancellationToken);

        // read file
        List<CsvLeadDto> records = await _csvReaderService.CsvToLeads(filename, cancellationToken);

        // campaign placeholder
        Domain.CampaignAggregate.Campaign? campaign = null;

        // if campaign id provided, get campaign
        if (request.CampaignId != null)
        {
            campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create((Guid)request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<UploadLeadsResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }
        }

        // Same CSV / same upload: EF query may not see CampaignLeads added earlier in this loop before SaveChanges.
        HashSet<Guid> leadIdsAlreadyLinkedToCampaignThisUpload = [];

        foreach (CsvLeadDto record in records)
        {
            // if phone number is not valid, ignore record
            if (!Regex.Match(record.PhoneNumber, RegexHelper.PHONE_NUMBER_REGEX).Success)
            {
                continue;
            }

            Domain.LeadAggregate.Lead lead = await LeadHelper.CreateOrUpdate(
                new(
                    request.CompanyId,
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
                CompanyId.Create(request.CompanyId),
                _leadRepository);

            if (campaign is null)
            {
                continue;
            }

            Guid leadKey = lead.Id.Value;
            if (leadIdsAlreadyLinkedToCampaignThisUpload.Contains(leadKey))
            {
                continue;
            }

            Domain.CampaignLeadAggregate.CampaignLead? existingLink =
                await _campaignLeadRepository.GetByCampaignAndLeadId(campaign.Id, lead.Id, cancellationToken);

            if (existingLink is not null)
            {
                leadIdsAlreadyLinkedToCampaignThisUpload.Add(leadKey);
                continue;
            }

            _campaignLeadRepository.Add(new Domain.CampaignLeadAggregate.CampaignLead(lead.Id, campaign.Id));
            leadIdsAlreadyLinkedToCampaignThisUpload.Add(leadKey);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadLeadsResponse(filename);
    }
}
