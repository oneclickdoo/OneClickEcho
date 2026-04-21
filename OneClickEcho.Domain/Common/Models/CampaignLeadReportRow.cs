namespace OneClickEcho.Domain.Common.Models;

/// <summary>One row of the campaign lead report (joined campaign lead + lead phone / unsubscribe).</summary>
public sealed class CampaignLeadReportRow
{
    public string PhoneNumber { get; init; } = string.Empty;

    public short ViberStatus { get; init; }

    public string? ViberStatusDescription { get; init; }

    public short SmsStatus { get; init; }

    public string? SmsStatusDescription { get; init; }

    public bool IsUnsubscribed { get; init; }
}
