using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.PauseCampaign
{
    public class PauseCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<PauseCampaignCommand, PauseCampaignResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<PauseCampaignResponse>> Handle(PauseCampaignCommand request, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<PauseCampaignResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            if (campaign.Status != CampaignStatus.Queued && campaign.Status != CampaignStatus.PreparingLaunch)
            {
                return Result.Failure<PauseCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    $"The Campaign with Id:\"{request.CampaignId}\" cannot be paused (not queued or preparing)."
                ));
            }

            // pause campaign
            campaign.Status = CampaignStatus.Draft;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PauseCampaignResponse(campaign.Id.Value);
        }
    }
}
