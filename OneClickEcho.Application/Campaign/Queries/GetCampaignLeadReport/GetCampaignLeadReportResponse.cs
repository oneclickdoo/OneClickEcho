using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.Common.Models;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadReport;

public sealed class GetCampaignLeadReportResponse(IPagedList<CampaignLeadReportRow> items)
    : PagedListDto<CampaignLeadReportRow, CampaignLeadReportItemDto>(items)
{
    public override List<CampaignLeadReportItemDto> ConvertTToTDto(List<CampaignLeadReportRow> rows)
    {
        List<CampaignLeadReportItemDto> result = new(rows.Count);
        foreach (CampaignLeadReportRow r in rows)
        {
            result.Add(new CampaignLeadReportItemDto(
                r.PhoneNumber,
                r.ViberStatus,
                r.ViberStatusDescription,
                r.SmsStatus,
                r.SmsStatusDescription,
                r.IsUnsubscribed));
        }

        return result;
    }
}
