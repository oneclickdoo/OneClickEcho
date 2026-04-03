using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Commands.CompleteCampaign;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class SendCampaignMessagesJob(IMessageSendingService messageSendingService, IMediator mediator) : IJob
{
    private readonly IMessageSendingService _messageSendingService = messageSendingService;
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        Guid campaignId = context.JobDetail.JobDataMap.GetGuid("CampaignId");

        Console.WriteLine(DateTime.Now + " - Running SendCampaignMessages job for campaign ID: " + campaignId);

        await _messageSendingService.SendMessagesForCampaignId(CampaignId.Create(campaignId));

        await _mediator.Send(new CompleteCampaignCommand(campaignId));

        Console.WriteLine(DateTime.Now + " - SendCampaignMessages job for campaign ID: " + campaignId + " completed successfully.");
    }
}