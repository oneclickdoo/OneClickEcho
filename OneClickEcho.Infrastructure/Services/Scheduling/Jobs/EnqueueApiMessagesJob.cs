using MediatR;
using OneClickEcho.Application.Scheduling.Commands.EnqueueApiMessages;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.Common.Shared;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class EnqueueApiMessagesJob(
    IMediator mediator,
    ISchedulerFactory schedulerFactory) : IJob
{
    private readonly IMediator _mediator = mediator;
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine($"{DateTime.Now} - Running EnqueueApiMessagesJob...");
        
        Result<EnqueueApiMessagesResponse> response = await _mediator.Send(new EnqueueApiMessagesCommand());

        if (!response.Value.ApiMessages.Any())
        {
            // Console.WriteLine($"{DateTime.Now} - EnqueueApiMessagesJob found no unsent API messages.");
            return;
        }

        // Group messages by Sender and MessageType
        // TODO: Group by company as well
        var messageGroups = response.Value.ApiMessages
            .GroupBy(m => new { m.CompanyId, m.MessageType })
            .Where(g => g.Any())
            .ToList();

        foreach (var group in messageGroups)
        {
            string jobIdentityBase = $"send-api-messages-{group.Key.CompanyId}-{group.Key.MessageType}-{DateTime.Now.Ticks}";

            // Create a trigger that executes immediately
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobIdentityBase}-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                .Build();

            // Create the job and pass the batch data
            IJobDetail job = JobBuilder.Create<SendApiMessagesJob>()
                .WithIdentity($"{jobIdentityBase}-job")
                .UsingJobData("CompanyId", group.Key.CompanyId.Value)
                .UsingJobData("MessageType", (int)group.Key.MessageType)
                .Build();

            // Get the scheduler and schedule the job
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.ScheduleJob(job, trigger);

            // Console.WriteLine($"{DateTime.Now} - Scheduled SendApiMessagesJob for sender [{group.Key.CompanyId}] and message type [{group.Key.MessageType}] with {group.Count()} messages");
        }

        // Console.WriteLine($"{DateTime.Now} - EnqueueApiMessagesJob completed successfully. Scheduled {messageGroups.Count} sending jobs.");
    }
}