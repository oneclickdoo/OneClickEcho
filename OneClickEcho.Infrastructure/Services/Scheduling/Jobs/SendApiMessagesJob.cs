using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Common.Services.SmsService;
using OneClickEcho.Application.Common.Services.ViberService;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class SendApiMessagesJob(
    IMessageSendingService messageSendingService,
    IApiMessageRepository apiMessageRepository) : IJob
{
    private readonly IMessageSendingService _messageSendingService = messageSendingService;
    private readonly IApiMessageRepository _apiMessageRepository = apiMessageRepository;

    public async Task Execute(IJobExecutionContext context)
    {
        // Get CompanyId & MessageType (Viber / SMS)
        Guid companyIdGuid = context.JobDetail.JobDataMap.GetGuid("CompanyId");
        CompanyId companyId = CompanyId.Create(companyIdGuid);
        ApiMessageType messageType = (ApiMessageType)context.JobDetail.JobDataMap.GetInt("MessageType");

        // Console.WriteLine($"{DateTime.Now} - Running SendApiMessagesJob for Company ID [{companyId}] and message type [{messageType}]");

        // Get unsent messages for this sender and type
        List<ApiMessage> messages = await _apiMessageRepository.GetUnsentApiMessages(DateTime.Now.AddHours(-1));
        var groupMessages = messages.Where(m => m.CompanyId == companyId && m.MessageType == messageType).ToList();

        if (groupMessages.Count == 0)
        {
            // Console.WriteLine($"{DateTime.Now} - No messages found for sender [{companyId}] and message type [{messageType}]");
            return;
        }
        
        await _messageSendingService.SendApiMessages(companyId, groupMessages, messageType);

        // Console.WriteLine($"{DateTime.Now} - SendApiMessagesJob completed successfully for sender [{companyId}] and message type [{messageType}]. Sent {groupMessages.Count} messages.");
    }
}