using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.DeleteCampaign;

public class DeleteCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCampaignCommand, DeleteCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<DeleteCampaignResponse>> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        // get campaign
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<DeleteCampaignResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
        }

        if (campaign.Status == CampaignStatus.Done)
        {
            return Result.Failure<DeleteCampaignResponse>(new Error(
                "Campaign.BadRequest",
                $"The Campaign with Id:\"{request.CampaignId}\" is already done."
                ));
        }

        // delete campaign
        _campaignRepository.Delete(campaign);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteCampaignResponse());
    }
}