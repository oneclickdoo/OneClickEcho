using MediatR;
using OneClickEcho.Application.Scheduling.Commands.ExpirePendingMessages;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ExpirePendingViberMessagesJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator;
    
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"{DateTime.Now} - Running ExpirePendingViberMessagesJob...");
        
        var response = await _mediator.Send(new ExpirePendingMessagesCommand());
        
        Console.WriteLine($"{DateTime.Now} - ExpirePendingViberMessagesJob completed successfully. {response.Value.MessageCount} messages marked as expired.");
    }
}