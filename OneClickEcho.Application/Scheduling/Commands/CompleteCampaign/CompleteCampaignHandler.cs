using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;

public class CompleteCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteCampaignCommand, CompleteCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CompleteCampaignResponse>> Handle(CompleteCampaignCommand request, CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<CompleteCampaignResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
            ));
        }

        campaign.Status = CampaignStatus.Done;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompleteCampaignResponse(campaign);
    }
}