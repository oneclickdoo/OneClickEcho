using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Helpers;
using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;
using OneClickEcho.Application.Common.Viber;
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
    IOptions<ViberSettings> viberSettings, IOptions<PublicUploadsSettings> publicUploadsSettings,
    ISchedulerFactory schedulerFactory) : IMessageSendingService
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
    private readonly IOptions<PublicUploadsSettings> _publicUploadsSettings = publicUploadsSettings;
    private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

    /// <summary>Public base for <c>/uploads/…</c> files. Comtrade fetches video/image from this host; wrong or legacy default → delivery substatus 28 (file not permitted). Set <c>PublicUploads:BaseUrl</c> (e.g. <c>https://viber.oneclick.rs/uploads</c>).</summary>
    private string ViberMediaPublicBaseUrl =>
        string.IsNullOrWhiteSpace(_publicUploadsSettings.Value.BaseUrl)
            ? "https://api.echo.oneclick.rs/uploads"
            : _publicUploadsSettings.Value.BaseUrl.Trim().TrimEnd('/');

    /// <summary>Schedules SMS delivery polling jobs if they are not already registered (avoids duplicate Quartz jobs on retries).</summary>
    private async Task EnsureSmsDeliveryJobsScheduledAsync(Campaign campaign)
    {
        IScheduler scheduler = await _schedulerFactory.GetScheduler();
        JobKey jobKey = new($"delivery-job-campaign-{campaign.Id.Value}");
        if (await scheduler.CheckExists(jobKey))
        {
            return;
        }

        await InitiateSmsDeliveryJob(campaign);
    }

    public async Task InitiateSmsDeliveryJob(Campaign campaign)
    {
        DateTimeOffset jobDateTime = campaign.SendingType switch
        {
            CampaignSendingType.Immediate => DateTimeOffset.UtcNow.AddMinutes(1),
            CampaignSendingType.ScheduledDateTime =>
                new DateTimeOffset(campaign.SendingDatetime.ToUniversalTime(), TimeSpan.Zero).AddMinutes(1),
            _ => DateTimeOffset.UtcNow.AddMinutes(1),
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

        // Console.WriteLine(DateTime.UtcNow + $" - Successfully scheduled SmsDeliveryJob for upcoming campaign: {campaign.Id.Value} - {campaign.Name} for {jobDateTime:O}");
    }

    public async Task SendMessagesForCampaignId(CampaignId campaignId, List<Lead>? leads = null,
        bool viberOnlyForProvidedLeads = false, bool smsOnlyForProvidedLeads = false)
    {
        // get campaign
        Campaign campaign = await _campaignRepository.GetByIdAsync(campaignId)
             ?? throw new Exception($"Campaign [{campaignId}] not found.");

        if (viberOnlyForProvidedLeads && smsOnlyForProvidedLeads)
        {
            throw new Exception($"Campaign [{campaign.Id.Value}] - cannot combine viber-only and sms-only retry.");
        }

        // campaign must have a selected channel (Viber or SMS)
        ValidateCampaignChannels(campaign);

        if (viberOnlyForProvidedLeads)
        {
            if (!campaign.IsViber)
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - viber-only retry requires Viber channel.");
            }

            if (leads is null || leads.Count == 0)
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - viber-only retry requires a non-empty lead list.");
            }
        }

        if (smsOnlyForProvidedLeads)
        {
            if (!campaign.IsSms)
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - sms-only retry requires SMS channel.");
            }

            if (leads is null || leads.Count == 0)
            {
                throw new Exception($"Campaign [{campaign.Id.Value}] - sms-only retry requires a non-empty lead list.");
            }
        }

        // get leads
        leads ??= await _campaignLeadRepository.GetAllLeadsByCampaignIdAsync(campaignId);

        leads = leads.Where(l => !l.IsBlacklisted && !l.IsUnsubscribed).ToList();

        // check if there is leads
        if (leads.Count == 0)
        {
            throw new Exception($"Campaign [{campaign.Id.Value}] - doesn't have any leads.");
        }

        // Viber channel
        if (campaign.IsViber && !smsOnlyForProvidedLeads)
        {
            await ViberSendingService.SendViberMessagesToLeads(
                campaign,
                leads,
                _httpClientFactory,
                _viberSettings,
                ViberMediaPublicBaseUrl,
                _campaignLeadRepository,
                _stringTemplatingService,
                _unitOfWork);
        }

        // SMS channel
        if (campaign.IsSms && !viberOnlyForProvidedLeads)
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

            await EnsureSmsDeliveryJobsScheduledAsync(campaign);
        }
    }

    public async Task SendApiMessages(CompanyId companyId, List<ApiMessage> apiMessages, ApiMessageType messageType)
    {
        List<ApiMessage> allowed = await FilterApiMessagesNotBlockedAsync(companyId, apiMessages);
        if (allowed.Count == 0)
        {
            // Console.WriteLine($"{DateTime.Now} - SendApiMessages: all {apiMessages.Count} messages skipped (blacklist / unsubscribe).");
            return;
        }

        if (messageType == ApiMessageType.Viber)
        {
            await ViberSendingService.SendApiViberMessages(
                allowed,
                _httpClientFactory,
                _viberSettings,
                ViberMediaPublicBaseUrl,
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
                _viberSettings,
                ViberMediaPublicBaseUrl);
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

            ViberSendMessageType outboundType = ViberService.DetermineMessageType(campaign);

            if (outboundType == ViberSendMessageType.OneWaySurveyList)
            {
                ViberSurveyOptionsHelper.ParseRequired(campaign.ViberSurveyOptionsJson);
                if (string.IsNullOrWhiteSpace(campaign.ViberMessage))
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Viber survey requires intro message text (MessageText).");
                }

                if (!campaign.IsViberReceivable)
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Survey campaigns require „Enable responses” (two-way / receivable).");
                }
            }
            else if (outboundType == ViberSendMessageType.OneWayFile)
            {
                if (string.IsNullOrEmpty(campaign.ViberMedia) ||
                    !MediaHelper.TryGetViberDocumentFileType(campaign.ViberMedia, out _))
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Viber file message requires an uploaded or URL document with an allowed extension.");
                }
            }
            else if (outboundType is ViberSendMessageType.OneWayVideo
                     or ViberSendMessageType.OneWayVideoText
                     or ViberSendMessageType.OneWayVideoTextButton
                     or ViberSendMessageType.OneWayVideoTextActionButton)
            {
                if (string.IsNullOrEmpty(campaign.ViberVideoThumbnail))
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Viber video requires a thumbnail image URL or upload.");
                }

                if (outboundType != ViberSendMessageType.OneWayVideoTextActionButton)
                {
                    if (campaign.ViberFileSize is null || campaign.ViberVideoDuration is null)
                    {
                        throw new Exception(
                            $"Campaign [{campaign.Id.Value}] - Viber video (types 230–232) requires file size and duration (save messaging after setting hosted video metadata).");
                    }

                    int d = campaign.ViberVideoDuration.Value;
                    if (d < 1 || d > ViberVideoConstraints.MaxDurationSeconds)
                    {
                        throw new Exception(
                            $"Campaign [{campaign.Id.Value}] - Viber video duration must be 1–{ViberVideoConstraints.MaxDurationSeconds} seconds. Current: {d} s.");
                    }
                }
            }
            else if (CampaignHasViberVideo(campaign))
            {
                if (string.IsNullOrEmpty(campaign.ViberVideoThumbnail))
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Viber video requires a thumbnail image URL or upload.");
                }

                ViberSendMessageType viberMessageType = ViberService.DetermineMessageType(campaign);
                if (viberMessageType != ViberSendMessageType.OneWayVideoTextActionButton)
                {
                    if (campaign.ViberFileSize is null || campaign.ViberVideoDuration is null)
                    {
                        throw new Exception(
                            $"Campaign [{campaign.Id.Value}] - Viber video (types 230–232) requires file size and duration (save messaging tab after the preview shows duration, or re-upload the MP4).");
                    }

                    int d = campaign.ViberVideoDuration.Value;
                    if (d < 1 || d > ViberVideoConstraints.MaxDurationSeconds)
                    {
                        throw new Exception(
                            $"Campaign [{campaign.Id.Value}] - Viber video duration must be 1–{ViberVideoConstraints.MaxDurationSeconds} seconds (provider limit). Current: {d} s.");
                    }
                }

                if (IsPromoViberVideoOnly(campaign) && string.IsNullOrEmpty(campaign.ViberVideoThumbnail))
                {
                    throw new Exception(
                        $"Campaign [{campaign.Id.Value}] - Viber video-only (promotional, message type 230) requires a thumbnail image.");
                }
            }
            else if (outboundType == ViberSendMessageType.OneWayImageOnly)
            {
                if (string.IsNullOrEmpty(campaign.ViberMedia))
                {
                    throw new Exception($"Campaign [{campaign.Id.Value}] - Viber image message requires image media.");
                }
            }
            else if (string.IsNullOrWhiteSpace(campaign.ViberMessage))
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

    private static bool CampaignHasViberVideo(Campaign campaign)
    {
        if (string.IsNullOrEmpty(campaign.ViberMedia))
        {
            return false;
        }

        try
        {
            return MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Video;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPromoViberVideoOnly(Campaign campaign)
    {
        if (string.IsNullOrEmpty(campaign.ViberMedia) || !string.IsNullOrEmpty(campaign.ViberButtonUrl))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(campaign.ViberMessage))
        {
            return false;
        }

        try
        {
            return MediaHelper.GetMediaType(campaign.ViberMedia) == CampaignMediaType.Video;
        }
        catch
        {
            return false;
        }
    }
}