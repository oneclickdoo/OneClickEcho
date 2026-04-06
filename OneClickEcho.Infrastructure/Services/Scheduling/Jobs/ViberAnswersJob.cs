using OneClickEcho.Application.Common.Services;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ViberAnswersJob(IMessageDeliveryService messageDeliveryService) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine(DateTime.Now + " - Running ViberAnswersJob job...");

        await _messageDeliveryService.GetViberAnswersForLast49Hours();
        
        // Console.WriteLine(DateTime.Now + " - ViberAnswersJob job completed successfully.");
    }
}