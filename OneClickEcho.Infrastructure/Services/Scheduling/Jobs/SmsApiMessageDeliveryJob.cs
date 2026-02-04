using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Queries.GetSentMessages;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class SmsApiMessageDeliveryJob(IMessageDeliveryService messageDeliveryService, IMediator mediator) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"{DateTime.Now} - Running SmsApiMessageDeliveryJob...");
        
        // Get CompanyId
        Guid companyIdGuid = context.JobDetail.JobDataMap.GetGuid("CompanyId");
        CompanyId companyId = CompanyId.Create(companyIdGuid);

        var response = await _mediator.Send(new GetSentMessagesQuery());
        
        var smsMessages = response.Value.ApiMessages
            .Where(m => m.MessageType == ApiMessageType.Sms)
            .GroupBy(m => m.CompanyId)
            .ToList();

        foreach (var companyGroup in smsMessages)
        {
            await _messageDeliveryService.GetSmsDeliveryForApiMessages(companyId, companyGroup.ToList());
        }

        Console.WriteLine($"{DateTime.Now} - SmsApiMessageDeliveryJob completed successfully.");
    }
} 