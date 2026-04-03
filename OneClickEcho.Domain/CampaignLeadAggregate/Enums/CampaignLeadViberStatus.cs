namespace OneClickEcho.Domain.CampaignLeadAggregate.Enums;

public enum CampaignLeadViberStatus : short
{
    None = 0,
    Received = 1,
    Pending = 2,
    Delivered = 3,
    Seen = 4,
    Undelivered = 5,
    Expired = 6,
    Clicked = 7
}