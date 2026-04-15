using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.CloneCampaign
{
    public class CloneCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
        : ICommandHandler<CloneCampaignCommand, CloneCampaignResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<CloneCampaignResponse>> Handle(CloneCampaignCommand request, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<CloneCampaignResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                    ));
            }

            // clone campaign
            Domain.CampaignAggregate.Campaign newCampaign = new(
                CampaignId.CreateUnique(),
                Domain.CampaignAggregate.Enums.CampaignStatus.Draft,
                campaign.CompanyId,
                $"{campaign.Name} - Copy",
                campaign.IsViber,
                campaign.FallbackToSMS,
                campaign.IsViberReceivable,
                campaign.ViberSender,
                campaign.ViberMessage,
                campaign.ViberMedia,
                campaign.ViberButtonUrl,
                campaign.ViberButtonUrlTitle,
                campaign.ViberFileSize,
                campaign.ViberVideoThumbnail,
                campaign.ViberVideoDuration,
                campaign.IsSms,
                campaign.SmsSender,
                campaign.SmsMessage,
                campaign.TestPhoneNumber,
                campaign.SendingType,
                campaign.SendingDatetime
                );

            _campaignRepository.Add(newCampaign);

            newCampaign.ViberContentKind = campaign.ViberContentKind;
            newCampaign.ViberSurveyOptionsJson = campaign.ViberSurveyOptionsJson;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CloneCampaignResponse(newCampaign.Id.Value);
        }
    }
}
