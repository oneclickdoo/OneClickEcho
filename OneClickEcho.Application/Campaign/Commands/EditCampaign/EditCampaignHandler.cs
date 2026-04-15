using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Commands.EditCampaign;

public class EditCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<EditCampaignCommand, EditCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<EditCampaignResponse>> Handle(EditCampaignCommand request, CancellationToken cancellationToken)
    {
        // get campaign
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<EditCampaignResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
            ));
        }

        // update
        campaign.Name = request.Name;
        campaign.IsSms = request.IsSms;
        campaign.IsViber = request.IsViber;
        campaign.FallbackToSMS = request.FallbackToSMS;
        campaign.IsViberReceivable = request.IsViberReceivable;
        campaign.ViberSender = request.ViberSender;
        campaign.ViberMessage = request.ViberMessage;
        campaign.ViberMedia = request.ViberMedia;
        campaign.ViberButtonUrl = request.ViberButtonUrl;
        campaign.ViberButtonUrlTitle = request.ViberButtonUrlTitle;
        campaign.ViberVideoThumbnail = request.ViberVideoThumbnail;
        campaign.ViberFileSize = request.ViberFileSize;
        campaign.ViberVideoDuration = request.ViberVideoDuration;
        campaign.ViberValidity = request.ViberValidity;
        campaign.SmsSender = request.SmsSender;
        campaign.SmsMessage = request.SmsMessage;
        campaign.SendingType = request.SendingType;
        campaign.SendingDatetime = request.SendingDatetime ?? DateTime.UtcNow;
        campaign.TestPhoneNumber = request.TestPhoneNumber;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EditCampaignResponse(campaign.Id.Value);
    }
}