using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Models;
using OneClickEcho.Domain.Common.Queries;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadReport;

public sealed class GetCampaignLeadReportHandler(
    ICampaignRepository campaignRepository,
    ICampaignLeadRepository campaignLeadRepository)
    : IQueryHandler<GetCampaignLeadReportQuery, GetCampaignLeadReportResponse>
{
    public static readonly Error CampaignNotDone = new(
        "Campaign.LeadReport.NotDone",
        "The campaign lead report is only available when the campaign status is Done.");

    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;

    public async Task<Result<GetCampaignLeadReportResponse>> Handle(
        GetCampaignLeadReportQuery query,
        CancellationToken cancellationToken)
    {
        Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
            .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<GetCampaignLeadReportResponse>(new Error(
                "Campaign.NotFound",
                $"The Campaign with Id:\"{query.CampaignId}\" does not exist."));
        }

        if (campaign.Status != CampaignStatus.Done)
        {
            return Result.Failure<GetCampaignLeadReportResponse>(CampaignNotDone);
        }

        CampaignLeadViberStatus? viberFilter = query.ViberStatus is null
            ? null
            : (CampaignLeadViberStatus)query.ViberStatus.Value;

        CampaignLeadSMSStatus? smsFilter = query.SmsStatus is null
            ? null
            : (CampaignLeadSMSStatus)query.SmsStatus.Value;

        IPagedList<CampaignLeadReportRow> page = await _campaignLeadRepository.GetCampaignLeadReportAsync(
            campaign.Id,
            query.PhoneSearch,
            viberFilter,
            smsFilter,
            query.IsUnsubscribed,
            query,
            cancellationToken);

        return Result.Success(new GetCampaignLeadReportResponse(page));
    }
}
