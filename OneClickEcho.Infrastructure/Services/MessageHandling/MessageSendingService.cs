using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.Common;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Infrastructure.Services.MessageHandling.Sms;
using OneClickEcho.Infrastructure.Services.MessageHandling.Viber;
using OneClickEcho.Infrastructure.Services.Scheduling.Jobs;
using OneClickEcho.Infrastructure.Settings;
using Quartz;
using OneClickEcho.Domain.TestMessageAggregate;

namespace OneClickEcho.Infrastructure.Services.MessageHandling;

public class MessageSendingService(ICampaignRepository campaignRepository,
    ILeadRepository leadRepository, ICampaignLeadRepository campaignLeadRepository,
    ICompanyRepository companyRepository, IApiMessageRepository apiMessageRepository, IStringTemplatingService stringTemplatingService,
    IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork,
    IOptions<ViberSettings> viberSettings, ISchedulerFactory schedulerFactory) : IMessageSendingService
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IApiMessageRepository _apiMessageRepository = apiMessageRepository;
    private readonly IStringTemplatingService _stringTemplatingService = stringTemplatingService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOptions<ViberSettings> _viberSettings = viberSettings;
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

    public async Task InitiateSmsDeliveryJob(Campaign campaign)
    {
        DateTimeOffset jobDateTime = campaign.SendingType switch
        {
            CampaignSendingType.Immediate => new DateTimeOffset(DateTime.Now).AddMinutes(1),
            CampaignSendingType.ScheduledDateTime => new DateTimeOffset(campaign.SendingDatetime).AddMinutes(1),
            _ => new DateTimeOffset(DateTime.Now).AddMinutes(1),
        };

        ITrigger smsDeliveryTrigger = TriggerBuilder.Create()
            .WithIdentity($"delivery-trigger-campaign-{campaign.Id.Value}")
            .StartAt(jobDateTime)
            .WithSimpleSchedule(x => x
                .WithRepeatCount(14)
                .WithIntervalInMinutes(1))
            .Build();

        IJobDetail smsDeliveryJob = JobBuilder.Create<SmsDeliveryJob>()
            .WithIdentity($"delivery-job-campaign-{campaign.Id.Value}")
            .UsingJobData("CampaignId", campaign.Id.Value)
            .Build();
        
        ITrigger smsDeliverySixHourTrigger = TriggerBuilder.Create()
            .WithIdentity($"delivery-trigger-six-hour-campaign-{campaign.Id.Value}")
            // After the first 5 mins, check for two days every 6h
            .StartAt(jobDateTime.AddMinutes(15))
            .WithSimpleSchedule(x => x
                .WithRepeatCount(23)
                .WithIntervalInHours(1))
            .Build();

        IJobDetail smsDeliverySixHourJob = JobBuilder.Create<SmsDeliveryJob>()
            .WithIdentity($"delivery-job-six-hour-campaign-{campaign.Id.Value}")
            .UsingJobData("CampaignId", campaign.Id.Value)
            .Build();

        IScheduler scheduler = await _schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(smsDeliveryJob, smsDeliveryTrigger);
        await scheduler.ScheduleJob(smsDeliverySixHourJob, smsDeliverySixHourTrigger);

        Console.WriteLine(DateTime.Now + $" - Successfully scheduled SmsDeliveryJob for upcoming campaign: {campaign.Id.Value} - {campaign.Name} for {jobDateTime.ToLocalTime()}");
    }

    public async Task SendMessagesForCampaignId(CampaignId campaignId, List<Lead>? leads = null)
    {
        // get campaign
        Campaign campaign = await _campaignRepository.GetByIdAsync(campaignId)
             ?? throw new Exception($"Campaign [{campaignId}] not found.");

        // campaign must have a selected channel (Viber or SMS)
        ValidateCampaignChannels(campaign);

        // get leads
        leads ??= await _campaignLeadRepository.GetAllLeadsByCampaignIdAsync(campaignId);

        leads = leads.Where(l => !l.IsBlacklisted && !l.IsUnsubscribed).ToList();

        // check if there is leads
        if (leads.Count == 0)
        {
            throw new Exception($"Campaign [{campaign.Id.Value}] - doesn't have any leads.");
        }

        // Viber channel
        if (campaign.IsViber)
        {
            await ViberSendingService.SendViberMessagesToLeads(
                campaign,
                leads,
                _httpClientFactory,
                _viberSettings,
                _campaignLeadRepository,
                _stringTemplatingService);
        }

        // SMS channel
        if (campaign.IsSms)
        {
            await SmsSendingService.SendSmsToLeads(
                campaign,
                leads,
                _companyRepository,
                _campaignRepository,
                _campaignLeadRepository,
                _httpClientFactory,
                _stringTemplatingService,
                _unitOfWork);

            await InitiateSmsDeliveryJob(campaign);
        }
    }

    public async Task SendApiMessages(CompanyId companyId, List<ApiMessage> apiMessages, ApiMessageType messageType)
    {
        List<ApiMessage> allowed = await FilterApiMessagesNotBlockedAsync(companyId, apiMessages);
        if (allowed.Count == 0)
        {
            Console.WriteLine($"{DateTime.Now} - SendApiMessages: all {apiMessages.Count} messages skipped (blacklist / unsubscribe).");
            return;
        }

        if (messageType == ApiMessageType.Viber)
        {
            await ViberSendingService.SendApiViberMessages(
                allowed,
                _httpClientFactory,
                _viberSettings,
                _unitOfWork);
        }
        else if (messageType == ApiMessageType.Sms)
        {
            await SmsSendingService.SendApiSmsMessages(
                companyId,
                allowed,
                _companyRepository,
                _httpClientFactory,
                _unitOfWork);
        }
    }

    private async Task<List<ApiMessage>> FilterApiMessagesNotBlockedAsync(CompanyId companyId, List<ApiMessage> messages)
    {
        HashSet<string> keys = messages
            .Select(m => PhoneNumberHelper.Standardize(m.PhoneNumber))
            .Where(k => !string.IsNullOrEmpty(k))
            .ToHashSet(StringComparer.Ordinal);

        if (keys.Count == 0)
        {
            return messages;
        }

        List<Lead> leads = await _leadRepository.GetLeadsByCompanyMatchingNormalizedPhonesAsync(companyId, keys.ToList());
        HashSet<string> blocked = leads
            .Where(l => l.IsBlacklisted || l.IsUnsubscribed)
            .Select(l => PhoneNumberHelper.Standardize(l.PhoneNumber))
            .ToHashSet(StringComparer.Ordinal);

        return messages
            .Where(m =>
            {
                string k = PhoneNumberHelper.Standardize(m.PhoneNumber);
                return string.IsNullOrEmpty(k) || !blocked.Contains(k);
            })
            .ToList();
    }

    public async Task SendTestMessages(Campaign campaign, TestMessage testMessage)
    {
        // campaign must have a selected channel (Viber or SMS)
        ValidateCampaignChannels(campaign);

        // Viber channel
        if (campaign.IsViber)
        {
            await ViberSendingService.SendViberMessagesToTestPhoneNumbers(
                campaign,
                testMessage,
                _httpClientFactory,
                _viberSettings);
        }

        // SMS channel
        if (campaign.IsSms)
        {
            await SmsSendingService.SendSmsToTestPhoneNumbers(
                campaign,
                testMessage,
                _companyRepository,
                _httpClientFactory,
                _unitOfWork);
        }
    }

    private static void ValidateCampaignChannels(Campaign campaign)
    {
        // check both channels
        if (!campaign.IsViber && !campaign.IsSms)
        {
            throw new Exception($"Campaign [{campaign.Id.Value}] - doesn't have selected channels.");
        }

        // check Viber channel
        if (campaign.IsViber)
        {
            // check if Viber sender is defined
            if (string.IsNullOrEmpty(campaign.ViberSender))
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - Viber sender is not defined.");
            }

            // check if Viber message is defined
            if (string.IsNullOrEmpty(campaign.ViberMessage))
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - Viber message is not defined.");
            }
        }

        // check SMS channel
        if (campaign.IsSms || campaign.FallbackToSMS)
        {
            // check if SMS sender is defined
            if (string.IsNullOrEmpty(campaign.SmsSender))
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - SMS sender is not defined.");
            }

            // check if SMS message is defined
            if (string.IsNullOrEmpty(campaign.SmsMessage))
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - SMS message is not defined.");
            }
        }
    }
}