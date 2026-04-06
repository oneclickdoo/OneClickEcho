using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Queries.GetSentMessages;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ApiMessageDeliveryJob(ISchedulerFactory schedulerFactory, IMediator mediator) : IJob
{
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;
    private readonly IMediator _mediator = mediator;
    
    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine($"{DateTime.Now} - Running ApiMessageDeliveryJob...");

        var response = await _mediator.Send(new GetSentMessagesQuery());
        
        var messageGroups = response.Value.ApiMessages
            .GroupBy(m => new { m.CompanyId, m.MessageType })
            .Where(g => g.Any())
            .ToList();

        foreach (var group in messageGroups)
        {
            string jobIdentityBase = $"delivery-api-messages-{group.Key.CompanyId}-{group.Key.MessageType}-{DateTime.Now.Ticks}";

            // Create a trigger that executes immediately
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobIdentityBase}-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                .Build();

            // Create the job based on message type
            IJobDetail job = group.Key.MessageType == ApiMessageType.Viber
                ? JobBuilder.Create<ViberApiMessageDeliveryJob>()
                    .UsingJobData("CompanyId", group.Key.CompanyId.Value)
                    .WithIdentity($"{jobIdentityBase}-job")
                    .Build()
                : JobBuilder.Create<SmsApiMessageDeliveryJob>()
                    .UsingJobData("CompanyId", group.Key.CompanyId.Value)
                    .WithIdentity($"{jobIdentityBase}-job")
                    .Build();

            // Get the scheduler and schedule the job
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.ScheduleJob(job, trigger);

            // Console.WriteLine($"{DateTime.Now} - Scheduled {group.Key.MessageType}ApiMessageDeliveryJob for company ID [{group.Key.CompanyId}] with {group.Count()} messages");
        }

        // Console.WriteLine($"{DateTime.Now} - ApiMessageDeliveryJob completed successfully. Scheduled {messageGroups.Count} delivery jobs.");
    }
}