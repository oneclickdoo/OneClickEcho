using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Common.Services.ViberService.Request;
using OneClickEcho.Application.Common.Services.ViberService.Request.Common;
using OneClickEcho.Application.Common.Services.ViberService.Response;
using OneClickEcho.Application.Common.Services.ViberService.Response.Common;
using OneClickEcho.Application.Common.Services.ViberService.Response.Enum;
using OneClickEcho.Application.Common.Viber;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Domain.TestMessageAggregate.Repositories;
using OneClickEcho.Infrastructure.Services.MessageHandling.Sms;
using OneClickEcho.Infrastructure.Settings;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber
{
    public class ViberDeliveryService
    {
        private static readonly int MAX_RECORDS_PER_REQUEST = 200;
        
        public static async Task GetViberDeliveryForLast49Hours(
            ICompanyRepository companyRepository,
            ICampaignRepository campaignRepository,
            ICampaignLeadRepository campaignLeadRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            ILeadRepository leadRepository,
            IStringTemplatingService stringTemplatingService,
            IUnitOfWork unitOfWork)
        {
            List<Campaign> last49HoursViberCampaigns = await campaignRepository.GetLast49HoursViberCampaigns();
            
            // create HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };

            // get all non-terminal status campaign leads to check
            List<CampaignLead> campaignLeads = await campaignLeadRepository
                .GetNonTerminalCampaignLeadsForCampaignIdsAsync(
                    last49HoursViberCampaigns.Select(x => x.Id).ToList()
                );

            Dictionary<long, CampaignLead> campaignLeadByViberMessageId = new();
            foreach (CampaignLead cl in campaignLeads)
            {
                if (cl.ViberMessageId <= 0)
                {
                    continue;
                }

                campaignLeadByViberMessageId.TryAdd(cl.ViberMessageId, cl);
            }

            // divide campaign leads into chunks
            List<List<CampaignLead>> dividedCampaignLeads = [];

            for (int i = 0; i < campaignLeads.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedCampaignLeads.Add(campaignLeads.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, campaignLeads.Count - i)));
            }

            int j = 0;

            Dictionary<CampaignId, List<LeadId>> undeliveredCampaignLeads = [];
            Dictionary<CampaignId, List<LeadId>> unsubscribedCampaignLeads = [];

            // send chunk by chunk
            foreach (List<CampaignLead> dividedCampaignLead in dividedCampaignLeads)
            {
                // Console.WriteLine(DateTime.Now + $" - Fetching delivery for for [{dividedCampaignLead.Count}] leads, index [{j} - {j + dividedCampaignLead.Count - 1}]. Total leads: [{campaignLeads.Count}]");

                j += dividedCampaignLead.Count;

                DeliveryViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    IDList = dividedCampaignLead.Select(x => x.ViberMessageId).ToList()
                };

                // send delivery request
                DeliveryViberMessageResponseDto? response = await viberService.DeliveryById(request)
                    ?? throw new Exception($"Failed to get delivery response.");

                // Console.WriteLine(DateTime.Now + $" - Matched [{response.ViberMessageResponses.Count}] leads during delivery.");

                // Keep only "extra" duplicate rows where MessageId + Status + SubStatus + ClickCount are the same
                // (skip the first/original row in each such group).
                // Different statuses for the same MessageId are delivery progression, not duplicates.
                List<ViberDeliveryEvent> duplicateDeliveryEvents = [];
                foreach (IGrouping<(long MessageId, DeliveryViberStatus Status, int SubStatus, int ClickCount), DeliveryViberMessageResponse> groupedByKey in
                         response.ViberMessageResponses.GroupBy(x => (
                             x.MessageId,
                             x.MessageStatus.Status,
                             (int)x.MessageStatus.SubStatus,
                             x.ClickInfo.ClickCount)))
                {
                    using IEnumerator<DeliveryViberMessageResponse> it = groupedByKey.GetEnumerator();
                    if (!it.MoveNext())
                    {
                        continue;
                    }

                    // Skip first row; record only duplicates.
                    while (it.MoveNext())
                    {
                        DeliveryViberMessageResponse duplicateItem = it.Current;
                        if (!campaignLeadByViberMessageId.TryGetValue(duplicateItem.MessageId, out CampaignLead? rawCampaignLead))
                        {
                            continue;
                        }

                        duplicateDeliveryEvents.Add(new ViberDeliveryEvent(
                            rawCampaignLead.Id,
                            duplicateItem.MessageId,
                            (short)duplicateItem.MessageStatus.Status,
                            (int)duplicateItem.MessageStatus.SubStatus,
                            duplicateItem.ClickInfo.ClickCount));
                    }
                }

                if (duplicateDeliveryEvents.Count > 0)
                {
                    string sampleDuplicateIds = string.Join(
                        ",",
                        duplicateDeliveryEvents
                            .Select(x => x.ViberMessageId)
                            .Distinct()
                            .Take(10));
                    Console.WriteLine(
                        $"{DateTime.UtcNow:O} - ViberDelivery duplicates detected: raw={response.ViberMessageResponses.Count}, " +
                        $"duplicates={duplicateDeliveryEvents.Count}, sampleMessageIds=[{sampleDuplicateIds}]");
                    await campaignLeadRepository.AddViberDeliveryEvents(duplicateDeliveryEvents);
                }
                else
                {
                    Console.WriteLine(
                        $"{DateTime.UtcNow:O} - ViberDelivery no duplicates in chunk: raw={response.ViberMessageResponses.Count}.");
                }

                List<DeliveryViberMessageResponse> mergedResponses =
                    DeliveryViberResponseDeduplicator.Deduplicate(response.ViberMessageResponses);
                if (mergedResponses.Count < response.ViberMessageResponses.Count)
                {
                    // Console.WriteLine(DateTime.Now +
                    //     $" - Deduplicated delivery rows: {response.ViberMessageResponses.Count} -> {mergedResponses.Count} (same MessageId).");
                }

                // check responses
                foreach (DeliveryViberMessageResponse item in mergedResponses)
                {
                    campaignLeadByViberMessageId.TryGetValue(item.MessageId, out CampaignLead? campaignLead);

                    // update campaign lead status
                    if (campaignLead != null)
                    {
                        // Console.WriteLine(DateTime.Now + $" - Updating delivery status for lead ViberMessageId [{item.MessageId}]. Old status: [{campaignLead.ViberStatus}]; New status: [{item.MessageStatus.Status}]");

                        CampaignLeadViberStatus deliveryStatus = (CampaignLeadViberStatus)item.MessageStatus.Status;
                        bool clicked = item.ClickInfo.ClickCount > 0;
                        CampaignLeadViberStatus newStatus = clicked ? CampaignLeadViberStatus.Clicked : deliveryStatus;

                        // Stale duplicate row (e.g. Pending without click) must not downgrade Clicked.
                        if (campaignLead.ViberStatus == CampaignLeadViberStatus.Clicked &&
                            newStatus != CampaignLeadViberStatus.Clicked)
                        {
                            continue;
                        }

                        campaignLead.ViberStatus = newStatus;

                        campaignLead.ViberStatusDescription =
                            CampaignLeadViberStatusDescriptions.ForDelivery(deliveryStatus, item.MessageStatus.SubStatus, clicked);

                        // check if message is undelivered
                        if ((CampaignLeadViberStatus)item.MessageStatus.Status == CampaignLeadViberStatus.Undelivered)
                        {
                            // SMS fallback already attempted (or in flight): do not queue again every minute while Viber stays Undelivered.
                            if (campaignLead.SMSStatus != CampaignLeadSMSStatus.None)
                            {
                                continue;
                            }

                            if (!undeliveredCampaignLeads.ContainsKey(campaignLead.CampaignId))
                            {
                                undeliveredCampaignLeads.Add(campaignLead.CampaignId, new List<LeadId>());
                            }

                            undeliveredCampaignLeads[campaignLead.CampaignId].Add(campaignLead.LeadId);
                        }

                        if (item.MessageStatus.SubStatus == DeliveryViberSubstatus.SRVC_USER_BLOCKED)
                        {
                            if (!unsubscribedCampaignLeads.ContainsKey(campaignLead.CampaignId))
                            {
                                unsubscribedCampaignLeads.Add(campaignLead.CampaignId, new List<LeadId>());
                            }
                            
                            unsubscribedCampaignLeads[campaignLead.CampaignId].Add(campaignLead.LeadId);
                        }
                    }
                    else
                    {
                        // Console.WriteLine(DateTime.Now + $" - Lead ViberMessageId [{item.MessageId}] not found during delivery update.");
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }

            // send fallback SMS messages to undelivered campaign leads
            foreach (CampaignId campaignId in undeliveredCampaignLeads.Keys)
            {
                Campaign? campaign = last49HoursViberCampaigns.FirstOrDefault(x => x.Id == campaignId);
                
                if (campaign == null) continue;
                
                if (!undeliveredCampaignLeads.ContainsKey(campaignId) || undeliveredCampaignLeads[campaignId].Count <= 0) continue;
                
                if (campaign.FallbackToSMS)
                {
                    List<LeadId> undeliveredIds = undeliveredCampaignLeads[campaignId];
                    if (unsubscribedCampaignLeads.TryGetValue(campaignId, out List<LeadId>? unsubIds))
                    {
                        HashSet<LeadId> unsubSet = unsubIds.ToHashSet();
                        undeliveredIds = undeliveredIds.Where(id => !unsubSet.Contains(id)).ToList();
                    }

                    if (undeliveredIds.Count <= 0)
                    {
                        continue;
                    }

                    // Console.WriteLine(DateTime.Now + $" - Found [{undeliveredIds.Count}] undelivered leads in campaign [{campaignId}]. Initializing SMS fallback...");
                    
                    List<Domain.LeadAggregate.Lead> leads = await leadRepository
                        .GetAllInLeadIdList(campaign.CompanyId, undeliveredIds);

                    leads = leads.Where(l => !l.IsBlacklisted && !l.IsUnsubscribed).ToList();
                    if (leads.Count <= 0)
                    {
                        continue;
                    }

                    await SmsSendingService.SendSmsToLeads(
                        campaign,
                        leads,
                        companyRepository,
                        campaignRepository,
                        campaignLeadRepository,
                        httpClientFactory,
                        stringTemplatingService,
                        unitOfWork);
                }
            }

            foreach (CampaignId campaignId in unsubscribedCampaignLeads.Keys)
            {
                Campaign? campaign = last49HoursViberCampaigns.FirstOrDefault(x => x.Id == campaignId);

                if (campaign == null) continue;
                
                if (!unsubscribedCampaignLeads.ContainsKey(campaignId) || unsubscribedCampaignLeads[campaignId].Count <= 0) continue;

                // mark leads as unsubscribed
                // Console.WriteLine(DateTime.Now + $" - Found [{unsubscribedCampaignLeads[campaignId].Count}] unsubscribed leads in campaign [{campaignId}]. Updating flags...");
                
                List<Domain.LeadAggregate.Lead> leads = await leadRepository
                    .GetAllInLeadIdList(campaign.CompanyId, unsubscribedCampaignLeads[campaignId]);

                foreach (Domain.LeadAggregate.Lead lead in leads)
                {
                    lead.IsUnsubscribed = true;
                    lead.IsBlacklisted = true;
                }
            }
            
            await unitOfWork.SaveChangesAsync();
        }
        
        public static async Task GetViberDeliveryForApiMessages(
            CompanyId companyId,
            List<ApiMessage> apiMessages,
            ICompanyRepository companyRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            ILeadRepository leadRepository,
            IUnitOfWork unitOfWork)
        {
            // create HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };

            // divide campaign leads into chunks
            List<List<ApiMessage>> dividedApiMessages = [];

            for (int i = 0; i < apiMessages.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedApiMessages.Add(apiMessages.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, apiMessages.Count - i)));
            }

            int j = 0;

            List<ApiMessage> undeliveredApiMessages = new List<ApiMessage>();
            List<ApiMessage> unsubscribedApiMessages = new List<ApiMessage>();

            // send chunk by chunk
            foreach (List<ApiMessage> dividedApiMessage in dividedApiMessages)
            {
                // Console.WriteLine(DateTime.Now + $" - Fetching delivery for for [{dividedApiMessage.Count}] Viber API messages, index [{j} - {j + dividedApiMessage.Count - 1}]. Total Viber API Messages: [{apiMessages.Count}]");

                j += dividedApiMessage.Count;

                DeliveryViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    IDList = dividedApiMessage.Select(x => x.ViberMessageId).ToList()
                };

                // send delivery request
                DeliveryViberMessageResponseDto? response = await viberService.DeliveryById(request)
                    ?? throw new Exception($"Failed to get delivery response.");

                // Console.WriteLine(DateTime.Now + $" - Matched [{response.ViberMessageResponses.Count}] Viber API messages during delivery.");

                List<DeliveryViberMessageResponse> mergedApiResponses =
                    DeliveryViberResponseDeduplicator.Deduplicate(response.ViberMessageResponses);
                if (mergedApiResponses.Count < response.ViberMessageResponses.Count)
                {
                    // Console.WriteLine(DateTime.Now +
                    //     $" - Deduplicated API delivery rows: {response.ViberMessageResponses.Count} -> {mergedApiResponses.Count}.");
                }

                // check responses
                foreach (DeliveryViberMessageResponse item in mergedApiResponses)
                {
                    // get campaign lead
                    ApiMessage? apiMessage = apiMessages.FirstOrDefault(x => x.ViberMessageId == item.MessageId);

                    // update campaign lead status
                    if (apiMessage != null)
                    {
                        // Console.WriteLine(DateTime.Now + $" - Updating delivery status for Viber API Message ViberMessageId [{item.MessageId}]. Old status: [{apiMessage.ViberStatus}]; New status: [{item.MessageStatus.Status}]");

                        CampaignLeadViberStatus deliveryStatus = (CampaignLeadViberStatus)item.MessageStatus.Status;
                        bool clicked = item.ClickInfo.ClickCount > 0;
                        CampaignLeadViberStatus newApiStatus = clicked ? CampaignLeadViberStatus.Clicked : deliveryStatus;

                        if (apiMessage.ViberStatus == CampaignLeadViberStatus.Clicked &&
                            newApiStatus != CampaignLeadViberStatus.Clicked)
                        {
                            continue;
                        }

                        apiMessage.ViberStatus = newApiStatus;

                        apiMessage.ViberStatusDescription =
                            CampaignLeadViberStatusDescriptions.ForDelivery(deliveryStatus, item.MessageStatus.SubStatus, clicked);

                        // check if message is undelivered
                        if ((CampaignLeadViberStatus)item.MessageStatus.Status == CampaignLeadViberStatus.Undelivered)
                        {
                            undeliveredApiMessages.Add(apiMessage);
                        }

                        if (item.MessageStatus.SubStatus == DeliveryViberSubstatus.SRVC_USER_BLOCKED)
                        {
                            unsubscribedApiMessages.Add(apiMessage);
                        }
                    }
                    else
                    {
                        // Console.WriteLine(DateTime.Now + $" - Viber API Message ViberMessageId [{item.MessageId}] not found during delivery update.");
                    }
                }
            }

            if (undeliveredApiMessages.Any())
            {
                // Console.WriteLine(DateTime.Now + $" - Found [{undeliveredApiMessages.Count}] undelivered Viber API messages.");
                
                List<ApiMessage> fallbackApiMessages = new List<ApiMessage>();
                
                // send fallback SMS messages to undelivered API messages
                foreach (ApiMessage apiMessage in undeliveredApiMessages)
                {
                    if (apiMessage.HasSmsFallback)
                    {
                        fallbackApiMessages.Add(apiMessage);
                    }
                }

                if (fallbackApiMessages.Any())
                {
                    // Console.WriteLine(DateTime.Now + $" - Found [{fallbackApiMessages.Count}] undelivered Viber API messages with fallback enabled. Initializing SMS fallback...");

                    HashSet<string> fbKeys = fallbackApiMessages
                        .Select(m => PhoneNumberHelper.Standardize(m.PhoneNumber))
                        .Where(k => !string.IsNullOrEmpty(k))
                        .ToHashSet(StringComparer.Ordinal);

                    List<ApiMessage> allowedFallback = fallbackApiMessages;
                    if (fbKeys.Count > 0)
                    {
                        List<Domain.LeadAggregate.Lead> fbLeads = await leadRepository.GetLeadsByCompanyMatchingNormalizedPhonesAsync(
                            companyId,
                            fbKeys.ToList());

                        HashSet<string> blocked = fbLeads
                            .Where(l => l.IsBlacklisted || l.IsUnsubscribed)
                            .Select(l => PhoneNumberHelper.Standardize(l.PhoneNumber))
                            .ToHashSet(StringComparer.Ordinal);

                        allowedFallback = fallbackApiMessages
                            .Where(m =>
                            {
                                string k = PhoneNumberHelper.Standardize(m.PhoneNumber);
                                return string.IsNullOrEmpty(k) || !blocked.Contains(k);
                            })
                            .ToList();
                    }

                    if (allowedFallback.Any())
                    {
                        await SmsSendingService.SendApiSmsMessages(
                            companyId,
                            allowedFallback,
                            companyRepository,
                            httpClientFactory,
                            unitOfWork);
                    }
                }
            }
            
            if (unsubscribedApiMessages.Count > 0)
            {
                // Console.WriteLine(DateTime.Now + $" - Found [{unsubscribedApiMessages.Count}] unsubscribed Viber API messages. Blacklisting matching leads...");

                HashSet<string> normalizedPhones = unsubscribedApiMessages
                    .Select(m => PhoneNumberHelper.Standardize(m.PhoneNumber))
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToHashSet(StringComparer.Ordinal);

                if (normalizedPhones.Count > 0)
                {
                    List<Domain.LeadAggregate.Lead> leads = await leadRepository.GetLeadsByCompanyMatchingNormalizedPhonesAsync(
                        companyId,
                        normalizedPhones.ToList());

                    foreach (Domain.LeadAggregate.Lead lead in leads)
                    {
                        lead.IsUnsubscribed = true;
                        lead.IsBlacklisted = true;
                    }
                }
            }
            
            await unitOfWork.SaveChangesAsync();
        }
        
        public static async Task GetViberTestDeliveryForLast48Hours(
            ITestMessageRepository testMessageRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            IUnitOfWork unitOfWork)
        {
            List<TestMessage> testMessages = await testMessageRepository.GetViberTestMessagesForLast48Hours();
            
            // create HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };

            // divide test messages into chunks
            List<List<TestMessage>> dividedTestMessages = [];

            for (int i = 0; i < testMessages.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedTestMessages.Add(testMessages.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, testMessages.Count - i)));
            }

            int j = 0;

            // send chunk by chunk
            foreach (List<TestMessage> dividedTestMessage in dividedTestMessages)
            {
                // Console.WriteLine(DateTime.Now + $" - Fetching test delivery for for [{dividedTestMessage.Count}] messages, index [{j} - {j + dividedTestMessage.Count - 1}]. Total test messages: [{testMessages.Count}]");

                j += dividedTestMessage.Count;

                DeliveryViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    IDList = dividedTestMessage.Select(x => x.ViberId).ToList()
                };

                // send delivery request
                DeliveryViberMessageResponseDto? response = await viberService.DeliveryById(request)
                    ?? throw new Exception($"Failed to get delivery response.");

                // Console.WriteLine(DateTime.Now + $" - Matched [{response.ViberMessageResponses.Count}] test messages during delivery.");

                List<DeliveryViberMessageResponse> mergedTestResponses =
                    DeliveryViberResponseDeduplicator.Deduplicate(response.ViberMessageResponses);
                if (mergedTestResponses.Count < response.ViberMessageResponses.Count)
                {
                    // Console.WriteLine(DateTime.Now +
                    //     $" - Deduplicated test delivery rows: {response.ViberMessageResponses.Count} -> {mergedTestResponses.Count}.");
                }

                // check responses
                foreach (DeliveryViberMessageResponse item in mergedTestResponses)
                {
                    // get campaign lead
                    TestMessage? testMessage = testMessages.FirstOrDefault(x => x.ViberId == item.MessageId);

                    // update campaign lead status
                    if (testMessage != null)
                    {
                        // Console.WriteLine(DateTime.Now + $" - Updating delivery status for test message ViberMessageId [{item.MessageId}].");
                        
                        if ((CampaignLeadViberStatus)item.MessageStatus.Status == CampaignLeadViberStatus.Delivered
                            || (CampaignLeadViberStatus)item.MessageStatus.Status == CampaignLeadViberStatus.Seen)
                        {
                            testMessage.IsDelivered = true;
                        }
                        
                        if (item.ClickInfo.ClickCount > 0)
                        {
                            testMessage.IsDelivered = true;
                            testMessage.IsClicked = true;
                        }
                    }
                    else
                    {
                        // Console.WriteLine(DateTime.Now + $" - Test message ViberMessageId [{item.MessageId}] not found during delivery update.");
                    }
                }
            }
            
            await unitOfWork.SaveChangesAsync();
        }
        
        public static async Task GetViberAnswersForLast49Hours(
            ICampaignRepository campaignRepository,
            ICampaignLeadRepository campaignLeadRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<ViberSettings> viberSettings,
            IUnitOfWork unitOfWork)
        {
            List<Campaign> last49HoursViberTwoWayCampaigns = await campaignRepository.GetLast49HoursViberTwoWayCampaigns();

            // create HTTP client
            HttpClient httpClient = httpClientFactory.CreateClient("ViberHttpClient");

            ViberService viberService = new(httpClient);

            ViberUserCredentials credentials = new()
            {
                UserName = viberSettings.Value.Username,
                Password = viberSettings.Value.Password
            };
            
            // get all answerable campaign leads to check
            List<CampaignLead> campaignLeads = await campaignLeadRepository
                .GetAnswerableCampaignLeadsForCampaignIdsAsync(
                    last49HoursViberTwoWayCampaigns.Select(x => x.Id).ToList()
                );

            // divide campaign leads into chunks
            List<List<CampaignLead>> dividedCampaignLeads = [];

            for (int i = 0; i < campaignLeads.Count; i += MAX_RECORDS_PER_REQUEST)
            {
                dividedCampaignLeads.Add(campaignLeads.GetRange(i, Math.Min(MAX_RECORDS_PER_REQUEST, campaignLeads.Count - i)));
            }

            int j = 0;

            List<ReceivedMessage> receivedMessages = [];

            // send chunk by chunk
            foreach (List<CampaignLead> dividedCampaignLead in dividedCampaignLeads)
            {
                // Console.WriteLine(DateTime.Now + $" - Fetching answers for [{dividedCampaignLead.Count}] leads, index [{j} - {j + dividedCampaignLead.Count - 1}]. Total leads: [{campaignLeads.Count}]");

                j += dividedCampaignLead.Count;

                AnswerViberMessageDto request = new()
                {
                    UserCredentials = credentials,
                    IDList = dividedCampaignLead.Select(x => x.ViberMessageId).ToList(),
                };

                // send answer request
                AnswerViberMessageResponseDto? response = await viberService.AnswersById(request)
                    ?? throw new Exception($"Failed to get answer response.");

                // check responses
                foreach (ViberAnswer entry in response.ViberAnswers)
                {
                    // get campaign lead
                    CampaignLead? campaignLead = campaignLeads.FirstOrDefault(x => x.ViberMessageId == entry.MessageId);

                    // update campaign lead status
                    if (campaignLead != null)
                    {
                        // Console.WriteLine(DateTime.Now + $" - Updating answers for lead ViberMessageId [{entry.MessageId}].");

                        receivedMessages.Add(new ReceivedMessage
                        {
                            CampaignLeadId = campaignLead.Id,
                            MessageContent = entry.MessageText
                        });
                    }
                    else
                    {
                        // Console.WriteLine(DateTime.Now + $" - Lead ViberMessageId [{entry.MessageId}] not found during answers update.");
                    }
                }
            }

            await campaignLeadRepository.AddReceivedMessages(receivedMessages);

            await unitOfWork.SaveChangesAsync();
        }
    }
}