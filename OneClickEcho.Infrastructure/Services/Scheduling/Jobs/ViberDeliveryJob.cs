using OneClickEcho.Application.Common.Services;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ViberDeliveryJob(IMessageDeliveryService messageDeliveryService) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine(DateTime.Now + " - Running ViberDeliveryJob job...");

        await _messageDeliveryService.GetViberDeliveryForLast49Hours();
        
        // Console.WriteLine(DateTime.Now + " - ViberDeliveryJob job completed successfully.");
    }
}