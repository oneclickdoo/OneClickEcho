using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.LeadCollectionAggregate.Repositories;

namespace OneClickEcho.Application.Campaign.Commands.LaunchCampaign;

public class LaunchCampaignHandler(
    ICampaignRepository campaignRepository,
    ILeadCollectionRepository leadCollectionRepository,
    IUnitOfWork unitOfWork,
    ICampaignLaunchScheduler campaignLaunchScheduler)
    : ICommandHandler<LaunchCampaignCommand, LaunchCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ILeadCollectionRepository _leadCollectionRepository = leadCollectionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICampaignLaunchScheduler _campaignLaunchScheduler = campaignLaunchScheduler;

    public async Task<Result<LaunchCampaignResponse>> Handle(LaunchCampaignCommand request, CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<LaunchCampaignResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."));
        }

        if (campaign.Status != CampaignStatus.Draft)
        {
            return Result.Failure<LaunchCampaignResponse>(new Error(
                "Campaign.BadRequest",
                "The campaign can only be launched from draft."));
        }

        if (!campaign.IsSms && !campaign.IsViber)
        {
            return Result.Failure<LaunchCampaignResponse>(new Error(
                "Campaign.BadRequest",
                "The Campaign does not have selected channel."));
        }

        List<Domain.LeadCollectionAggregate.LeadCollection> leadCollections = await _leadCollectionRepository
            .GetAllByCampaignIdAsync(campaign.Id, cancellationToken);

        if (leadCollections == null || leadCollections.Count == 0)
        {
            return Result.Failure<LaunchCampaignResponse>(new Error(
                "Campaign.BadRequest",
                "No lead collections found for this campaign."));
        }

        if (campaign.SendingType == CampaignSendingType.Immediate)
        {
            campaign.SendingDatetime = DateTime.UtcNow;
        }
        else
        {
            DateTime sendingUtc = campaign.SendingDatetime.ToUniversalTime();
            DateTime nowUtc = DateTime.UtcNow;

            if (nowUtc > sendingUtc)
            {
                return Result.Failure<LaunchCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    "The Campaign sending datetime is passed."));
            }
        }

        campaign.Status = CampaignStatus.PreparingLaunch;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _campaignLaunchScheduler.SchedulePrepareLaunchAsync(campaign.Id.Value, cancellationToken);

        return new LaunchCampaignResponse(campaign.Id.Value);
    }
}
