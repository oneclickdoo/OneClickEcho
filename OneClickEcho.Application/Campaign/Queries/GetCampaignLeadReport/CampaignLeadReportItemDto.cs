namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadReport;

public sealed record CampaignLeadReportItemDto(
    string PhoneNumber,
    short ViberStatus,
    string? ViberStatusDescription,
    short SmsStatus,
    string? SmsStatusDescription,
    bool IsUnsubscribed);
