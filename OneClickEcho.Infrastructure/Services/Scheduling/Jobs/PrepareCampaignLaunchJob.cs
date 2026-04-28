using MediatR;
using OneClickEcho.Application.Campaign.Commands.PrepareCampaignLaunch;
using OneClickEcho.Domain.Common.Shared;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

[DisallowConcurrentExecution]
public sealed class PrepareCampaignLaunchJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        Guid campaignId = context.JobDetail.JobDataMap.GetGuid("CampaignId");

        try
        {
            Result result = await _mediator.Send(
                new PrepareCampaignLaunchCommand(campaignId),
                context.CancellationToken);

            if (result.IsFailure)
            {
                Console.WriteLine(
                    $"{DateTime.UtcNow:O} PrepareCampaignLaunchJob failed for {campaignId}: {result.Error.Code} {result.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.UtcNow:O} PrepareCampaignLaunchJob exception for {campaignId}: {ex}");
            throw;
        }
    }
}
