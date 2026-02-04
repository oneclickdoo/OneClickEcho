using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Application.Campaign.Commands.AssignLeadCollection;

public class AssignLeadCollectionHandler(ICampaignRepository campaignRepository,
    ILeadCollectionRepository leadCollectionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AssignLeadCollectionCommand, AssignLeadCollectionResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<AssignLeadCollectionResponse>> Handle(AssignLeadCollectionCommand request, CancellationToken cancellationToken)
    {
        // get campaign
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<AssignLeadCollectionResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.LeadCollectionId}\" does not exist."
            ));
        }

        // get lead collection
        Domain.LeadCollectionAggregate.LeadCollection? leadCollection = await _leadCollectionRepository
            .GetByIdAsync(LeadCollectionId.Create(request.LeadCollectionId), cancellationToken);

        if (leadCollection is null)
        {
            return Result.Failure<AssignLeadCollectionResponse>(new Error(
                "LeadCollection.NotFound",
                $"The LeadCollection with Id:\"{request.LeadCollectionId}\" does not exist."
            ));
        }

        await _campaignRepository.AssignLeadCollectionToCampaign(new CampaignLeadCollection(
            CampaignId.Create(campaign.Id.Value),
            LeadCollectionId.Create(leadCollection.Id.Value)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignLeadCollectionResponse();
    }
}