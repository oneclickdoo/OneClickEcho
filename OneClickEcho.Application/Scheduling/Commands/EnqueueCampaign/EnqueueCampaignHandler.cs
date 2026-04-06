using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;

public class EnqueueCampaignHandler(ICampaignRepository campaignRepository)
    : ICommandHandler<EnqueueCampaignCommand, EnqueueCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;

    public async Task<Result<EnqueueCampaignResponse>> Handle(EnqueueCampaignCommand request, CancellationToken cancellationToken)
    {
        CampaignId id = CampaignId.Create(request.CampaignId);

        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(id, cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<EnqueueCampaignResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
            ));
        }

        bool claimed = await _campaignRepository.TryMarkAsInProgressFromQueuedAsync(id, cancellationToken);

        return Result.Success(new EnqueueCampaignResponse(claimed));
    }
}