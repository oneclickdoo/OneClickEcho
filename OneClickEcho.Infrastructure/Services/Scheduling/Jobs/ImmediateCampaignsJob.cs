using MediatR;
using OneClickEcho.Application.Scheduling.Commands.QueueImmediateCampaigns;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs
{
    public class ImmediateCampaignsJob(IMediator mediator) : IJob
    {
        private readonly IMediator _mediator = mediator;

        public async Task Execute(IJobExecutionContext context)
        {
            // Console.WriteLine(DateTime.Now + " - Running ImmediateCampaigns job...");

            // get immediate queued campaigns
            Domain.Common.Shared.Result<QueueImmediateCampaignsResponse> response = await _mediator
                .Send(new QueueImmediateCampaignsCommand());

            // Console.WriteLine($"{DateTime.Now} - Found {response.Value.ImmediateCampaigns.Count} immediate campaigns.");

            // schedule immediate campaigns
            foreach (Domain.CampaignAggregate.Campaign campaign in response.Value.ImmediateCampaigns)
            {
                // Console.WriteLine($"Scheduling immediate campaign: {campaign.Id.Value} - {campaign.Name}");

                ITrigger sendCampaignMessagesTrigger = TriggerBuilder.Create()
                    .WithIdentity($"send-trigger-campaign-{campaign.Id.Value}")
                    .StartAt(DateTimeOffset.UtcNow) // same instant regardless of server OS timezone
                    .WithSimpleSchedule(x => x.WithRepeatCount(0))
                    .Build();

                IJobDetail sendCampaignMessagesJob = JobBuilder.Create<SendCampaignMessagesJob>()
                    .WithIdentity($"send-job-campaign-{campaign.Id.Value}")
                    .UsingJobData("CampaignId", campaign.Id.Value)
                    .Build();

                await context.Scheduler.ScheduleJob(sendCampaignMessagesJob, sendCampaignMessagesTrigger);

                // Console.WriteLine($"Successfully scheduled immediate campaign: {campaign.Id.Value} - {campaign.Name}");
            }
        }
    }
}
