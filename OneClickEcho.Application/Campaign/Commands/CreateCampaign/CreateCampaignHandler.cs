using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Campaign.Commands.CreateCampaign;

public class CreateCampaignHandler(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCampaignCommand, CreateCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CreateCampaignResponse>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign campaign = new(
                name: request.CampaignName,
                companyId: CompanyId.Create(request.CompanyId),
                viberSender: request.ViberSender,
                isViber: request.IsViber,
                fallbackToSMS: request.FallbackToSMS,
                isViberReceivable: request.IsViberReceivable,
                viberMessage: request.ViberMessage,
                viberButtonUrl: request.ViberButtonUrl,
                viberButtonUrlTitle: request.ViberButtonUrlTitle,
                viberVideoThumbnail: request.ViberVideoThumbnail,
                isSms: request.IsSms,
                smsSender: request.SmsSender,
                smsMessage: request.SmsMessage,
                testPhoneNumber: request.TestPhoneNumbers,
                sendingType: request.SendingType,
                sendingDatetime: request.SendingDatetime,
                status: CampaignStatus.Draft
            );

        _campaignRepository.Add(campaign);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCampaignResponse(campaign.Id.Value);
    }
}