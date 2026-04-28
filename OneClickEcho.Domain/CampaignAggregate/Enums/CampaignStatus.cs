namespace OneClickEcho.Domain.CampaignAggregate.Enums;

public enum CampaignStatus : short
{
    Draft = 1,
    Queued = 2,
    InProgress = 3,
    Done = 4,
    /// <summary>Launch accepted; lead snapshot and queue transition run in a background job.</summary>
    PreparingLaunch = 5
}