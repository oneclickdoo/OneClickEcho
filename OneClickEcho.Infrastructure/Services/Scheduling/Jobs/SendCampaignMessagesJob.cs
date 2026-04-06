using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;
using OneClickEcho.Application.Scheduling.Commands.EnqueueCampaign;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

[DisallowConcurrentExecution]
public class SendCampaignMessagesJob(IMessageSendingService messageSendingService, IMediator mediator) : IJob
{
    private readonly IMessageSendingService _messageSendingService = messageSendingService;
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        Guid campaignId = context.JobDetail.JobDataMap.GetGuid("CampaignId");

        // Console.WriteLine(DateTime.Now + " - Running SendCampaignMessages job for campaign ID: " + campaignId);

        try
        {
            // InProgress only when send actually runs — avoids RetryPendingViberCampaignSendsJob firing
            // while a ScheduledDateTime campaign is still waiting on the Quartz trigger.
            Domain.Common.Shared.Result<EnqueueCampaignResponse> enqueue =
                await _mediator.Send(new EnqueueCampaignCommand(campaignId));
            if (enqueue.IsFailure)
            {
                Console.WriteLine(DateTime.Now + $" - SendCampaignMessages job: enqueue failed for {campaignId}: {enqueue.Error.Message}");
                return;
            }

            if (!enqueue.Value.ClaimedFromQueued)
            {
                // Console.WriteLine(DateTime.Now +
                //     $" - SendCampaignMessages job: skip duplicate send for campaign {campaignId} (not Queued or another instance already claimed).");
                return;
            }

            await _messageSendingService.SendMessagesForCampaignId(CampaignId.Create(campaignId));
            await _mediator.Send(new CompleteCampaignCommand(campaignId));

            // Console.WriteLine(DateTime.Now + " - SendCampaignMessages job for campaign ID: " + campaignId + " completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now + $" - SendCampaignMessages job FAILED for campaign ID: {campaignId}. Campaign left InProgress (not marked Done). {ex}");
            throw;
        }
    }
}