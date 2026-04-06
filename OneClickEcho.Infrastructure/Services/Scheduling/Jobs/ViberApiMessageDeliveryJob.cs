using MediatR;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Scheduling.Queries.GetSentMessages;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class ViberApiMessageDeliveryJob(IMessageDeliveryService messageDeliveryService, IMediator mediator) : IJob
{
    private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine($"{DateTime.Now} - Running ViberApiMessageDeliveryJob...");
        
        // Get CompanyId
        Guid companyIdGuid = context.JobDetail.JobDataMap.GetGuid("CompanyId");
        CompanyId companyId = CompanyId.Create(companyIdGuid);

        var response = await _mediator.Send(new GetSentMessagesQuery());
        
        var viberMessages = response.Value.ApiMessages
            .Where(m => m.MessageType == ApiMessageType.Viber)
            .GroupBy(m => m.CompanyId)
            .ToList();

        foreach (var companyGroup in viberMessages)
        {
            await _messageDeliveryService.GetViberDeliveryForApiMessages(companyId, companyGroup.ToList());
        }

        // Console.WriteLine($"{DateTime.Now} - ViberApiMessageDeliveryJob completed successfully.");
    }
} 