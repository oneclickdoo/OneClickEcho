using MediatR;
using OneClickEcho.Application.Scheduling.Commands.QueueUpcomingCampaigns;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class UpcomingCampaignsJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine(DateTime.Now + " - Running UpcomingCampaigns job...");

        // get upcoming campaigns
        Domain.Common.Shared.Result<QueueUpcomingCampaignsResponse> response = await _mediator
            .Send(new QueueUpcomingCampaignsCommand());

        // Console.WriteLine($"{DateTime.Now} - Found {response.Value.UpcomingCampaigns.Count} upcoming campaigns.");

        // schedule upcoming campaigns
        foreach (Domain.CampaignAggregate.Campaign campaign in response.Value.UpcomingCampaigns)
        {
            // Console.WriteLine($"Scheduling upcoming campaign: {campaign.Id.Value} - {campaign.Name}");

            DateTime sendingUtc = campaign.SendingDatetime.ToUniversalTime();
            ITrigger sendCampaignMessagesTrigger = TriggerBuilder.Create()
                .WithIdentity($"send-trigger-campaign-{campaign.Id.Value}")
                .StartAt(new DateTimeOffset(sendingUtc, TimeSpan.Zero))
                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                .Build();

            IJobDetail sendCampaignMessagesJob = JobBuilder.Create<SendCampaignMessagesJob>()
                .WithIdentity($"send-job-campaign-{campaign.Id.Value}")
                .UsingJobData("CampaignId", campaign.Id.Value)
                .Build();

            await context.Scheduler.ScheduleJob(sendCampaignMessagesJob, sendCampaignMessagesTrigger);

            // Console.WriteLine($"Successfully scheduled upcoming campaign: {campaign.Id.Value} - {campaign.Name} for {sendingUtc:O} (UTC)");
        }
    }
}