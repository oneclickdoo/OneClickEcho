using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.Campaign.Commands.CreateLeadCollectionFromStatus;

public class CreateLeadCollectionFromStatusHandler : ICommandHandler<CreateLeadCollectionFromStatusCommand, CreateLeadCollectionFromStatusResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCollectionFromStatusHandler(ICampaignRepository campaignRepository, 
        ICampaignLeadRepository campaignLeadRepository, ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _campaignLeadRepository = campaignLeadRepository;
        _leadCollectionRepository = leadCollectionRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<CreateLeadCollectionFromStatusResponse>> Handle(CreateLeadCollectionFromStatusCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);
        
        if (campaign is null)
        {
            return Result.Failure<CreateLeadCollectionFromStatusResponse>(new Error(
                "Campaign.NotFound",
                "Campaign not found."
            ));
        }

        var leads = await _campaignLeadRepository.GetLeadsByCampaignIdAndStatusAsync(
            campaign, request.ViberStatus, request.SmsStatus, cancellationToken);
        
        var leadIds = leads.Select(l => l.Id);

        var leadCollection =
            new Domain.LeadCollectionAggregate.LeadCollection(request.CollectionName,
                CompanyId.Create(request.CompanyId));
        
        leadCollection.AddLeadAssignments(leadIds);
        
        _leadCollectionRepository.Add(leadCollection);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLeadCollectionFromStatusResponse(leadCollection.Id.Value);
    }
}