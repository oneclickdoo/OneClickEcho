using OneClickEcho.Application.Common.Services;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ViberTestDeliveryJob(IMessageDeliveryService messageDeliveryService) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine(DateTime.Now + " - Running ViberTestDeliveryJob job...");

        await _messageDeliveryService.GetViberTestDeliveryForLast48Hours();
        
        Console.WriteLine(DateTime.Now + " - ViberTestDeliveryJob job completed successfully.");
    }
}