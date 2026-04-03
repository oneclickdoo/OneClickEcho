using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignById;

public class GetCampaignByIdHandler(ICampaignRepository campaignRepository)
    : IQueryHandler<GetCampaignByIdQuery, GetCampaignByIdResponse>
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;

    public async Task<Result<GetCampaignByIdResponse>> Handle(GetCampaignByIdQuery query,
        CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

        return campaign is null
            ? Result.Failure<GetCampaignByIdResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{query.CampaignId}\" does not exist."
            ))
            : (Result<GetCampaignByIdResponse>)new GetCampaignByIdResponse(
            campaign.Id.Value,
            campaign.CompanyId.Value,
            campaign.Status,
            campaign.Name,
            campaign.IsViber,
            campaign.FallbackToSMS,
            campaign.IsViberReceivable,
            campaign.ViberSender,
            campaign.ViberMessage,
            campaign.ViberMedia,
            campaign.ViberButtonUrl,
            campaign.ViberButtonUrlTitle,
            campaign.ViberVideoThumbnail,
            campaign.ViberValidity,
            campaign.IsSms,
            campaign.SmsSender,
            campaign.SmsMessage,
            campaign.TestPhoneNumber,
            campaign.SendingType,
            campaign.SendingDatetime
        );
    }
}