using OneClickEcho.Application.Common.Services;
using OneClickEcho.Infrastructure.Services.Scheduling.Jobs;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling;

public sealed class QuartzCampaignLaunchScheduler(ISchedulerFactory schedulerFactory) : ICampaignLaunchScheduler
{
    private const string PrepareJobGroup = "campaign-prepare";
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

    public async Task SchedulePrepareLaunchAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        JobKey jobKey = new($"prepare-launch-{campaignId}", PrepareJobGroup);
        if (await scheduler.CheckExists(jobKey, cancellationToken))
        {
            await scheduler.DeleteJob(jobKey, cancellationToken);
        }

        IJobDetail job = JobBuilder.Create<PrepareCampaignLaunchJob>()
            .WithIdentity(jobKey)
            .UsingJobData("CampaignId", campaignId)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"prepare-launch-tr-{campaignId}", PrepareJobGroup)
            .ForJob(jobKey)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }

    public async Task ScheduleImmediateSendJobAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        JobKey sendJobKey = new($"send-job-campaign-{campaignId}");
        if (await scheduler.CheckExists(sendJobKey, cancellationToken))
        {
            return;
        }

        ITrigger sendCampaignMessagesTrigger = TriggerBuilder.Create()
            .WithIdentity($"send-trigger-campaign-{campaignId}")
            .StartAt(DateTimeOffset.UtcNow)
            .WithSimpleSchedule(x => x.WithRepeatCount(0))
            .Build();

        IJobDetail sendCampaignMessagesJob = JobBuilder.Create<SendCampaignMessagesJob>()
            .WithIdentity(sendJobKey)
            .UsingJobData("CampaignId", campaignId)
            .Build();

        await scheduler.ScheduleJob(sendCampaignMessagesJob, sendCampaignMessagesTrigger, cancellationToken);
    }
}
