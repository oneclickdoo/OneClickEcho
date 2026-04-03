using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs
{
    public class SmsDeliveryJob(IMessageDeliveryService messageDeliveryService) : IJob
    {
        private readonly IMessageDeliveryService _messageDeliveryService = messageDeliveryService;

        public async Task Execute(IJobExecutionContext context)
        {
            Guid campaignId = context.JobDetail.JobDataMap.GetGuid("CampaignId");

            Console.WriteLine(DateTime.Now + " - Running SmsDeliveryJob job for campaign ID: " + campaignId);

            await _messageDeliveryService.GetSmsDeliveryForCampaignId(CampaignId.Create(campaignId));

            Console.WriteLine(DateTime.Now + " - SmsDeliveryJob job for campaign ID: " + campaignId + " completed successfully.");
        }
    }
}
