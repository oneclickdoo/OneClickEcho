namespace OneClickEcho.Application.Common.Services;

/// <summary>Schedules Quartz work for campaign launch (prepare leads, then immediate send).</summary>
public interface ICampaignLaunchScheduler
{
    Task SchedulePrepareLaunchAsync(Guid campaignId, CancellationToken cancellationToken = default);

    /// <summary>Schedules the immediate campaign send job at UTC now if not already scheduled.</summary>
    Task ScheduleImmediateSendJobAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
