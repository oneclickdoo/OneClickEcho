using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.EndCampaign
{
    public class EndCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<EndCampaignCommand, EndCampaignResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<EndCampaignResponse>> Handle(EndCampaignCommand request, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<EndCampaignResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            // check is campaign is in progress or sending is by date of birth
            if (campaign.Status != CampaignStatus.InProgress || campaign.SendingType != CampaignSendingType.ByDateOfBirth)
            {
                return Result.Failure<EndCampaignResponse>(new Error(
                    "Campaign.BadRquest",
                    $"The Campaign with Id:\"{request.CampaignId}\" is not in progress or sending is not by date of birth."
                ));
            }

            // complete campaign
            campaign.Status = CampaignStatus.Done;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new NotImplementedException();
        }
    }
}
