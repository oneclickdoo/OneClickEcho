using OneClickEcho.Application.Common.Services;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class SmsTestDeliveryJob(IMessageDeliveryService messageDeliveryService) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine(DateTime.Now + " - Running SmsTestDeliveryJob job...");

        await _messageDeliveryService.GetSmsTestDeliveryForLast48Hours();
        
        // Console.WriteLine(DateTime.Now + " - SmsTestDeliveryJob job completed successfully.");
    }
}