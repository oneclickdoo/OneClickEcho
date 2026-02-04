using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.Campaign.Commands.AddLeadsToCollectionFromStatus;

public class AddLeadsToCollectionFromStatusHandler : ICommandHandler<AddLeadsToCollectionFromStatusCommand, AddLeadsToCollectionFromStatusResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddLeadsToCollectionFromStatusHandler(ICampaignRepository campaignRepository, ICampaignLeadRepository campaignLeadRepository,
        ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _campaignLeadRepository = campaignLeadRepository;
        _leadCollectionRepository = leadCollectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AddLeadsToCollectionFromStatusResponse>> Handle(AddLeadsToCollectionFromStatusCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);
        
        if (campaign is null)
        {
            return Result.Failure<AddLeadsToCollectionFromStatusResponse>(new Error(
                "Campaign.NotFound",
                "Campaign not found."
            ));
        }
        
        var leadCollection = await _leadCollectionRepository.GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);
        
        if (leadCollection is null)
        {
            return Result.Failure<AddLeadsToCollectionFromStatusResponse>(new Error(
                "LeadCollection.NotFound",
                "LeadCollection not found."
            ));
        }
        
        var leads = await _campaignLeadRepository.GetLeadsByCampaignIdAndStatusAsync(
            campaign, request.ViberStatus, request.SmsStatus, cancellationToken);
        
        var leadIds = leads.Select(l => l.Id);
        
        leadCollection.AddLeadAssignments(leadIds);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddLeadsToCollectionFromStatusResponse();
    }
}