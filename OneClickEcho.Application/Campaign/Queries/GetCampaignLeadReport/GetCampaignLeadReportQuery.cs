using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadReport;

public sealed class GetCampaignLeadReportQuery : BasePagedQuery, IQuery<GetCampaignLeadReportResponse>
{
    public Guid CampaignId { get; set; }

    /// <summary>Substring match on lead phone (same idea as leads list search).</summary>
    public string? PhoneSearch { get; set; }

    public short? ViberStatus { get; set; }

    public short? SmsStatus { get; set; }

    /// <summary>When set, filter to unsubscribed (true) or not (false).</summary>
    public bool? IsUnsubscribed { get; set; }
}
