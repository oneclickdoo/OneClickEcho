using OneClickEcho.Application.Common.Services;
using OneClickEcho.Application.Common.Services.SmsService.Request;
using OneClickEcho.Application.Common.Services.SmsService.Response;
using OneClickEcho.Application.Common.Services.SmsService.Response.Enums;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.Common;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate;
using OneClickEcho.Domain.TestMessageAggregate;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Sms;

public class SmsSendingService
{
    // private static readonly int MAX_RECORDS_PER_REQUEST = 100;

    public static async Task SendSmsToLeads(
        Campaign campaign,
        List<Lead> leads,
        ICompanyRepository companyRepository,
        ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository,
        IHttpClientFactory httpClientFactory,
        IStringTemplatingService stringTemplatingService,
        IUnitOfWork unitOfWork)
    {
        // get company (for SMS credentials)
        Company company = await companyRepository.GetByIdAsync(campaign.CompanyId)
            ?? throw new Exception($"Company [{campaign.CompanyId.Value}] - not found.");

        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
        SmsService smsService = new(httpClient);

        leads = leads.Where(l => !l.IsBlacklisted && !l.IsUnsubscribed).ToList();
        if (leads.Count == 0)
        {
            return;
        }

        // send SMS to all leads one-by-one
        foreach (Lead lead in leads)
        {
            CampaignLead campaignLead = await campaignLeadRepository
                    .GetByCampaignAndLeadId(campaign.Id, lead.Id)
                    ?? throw new Exception($"CampaignLead for Lead [{lead.Id}] is not found.");

            if (campaignLead.SMSStatus != CampaignLeadSMSStatus.None)
            {
                // Console.WriteLine(
                //     $"{DateTime.UtcNow:O} - Skip SMS campaign {campaign.Id.Value} lead {lead.Id.Value}: SMSStatus={campaignLead.SMSStatus} (not None; avoids duplicate send).");
                continue;
            }

            // Reserve outbound slot before the HTTP call so the next ViberDeliveryJob cycle cannot enqueue the same lead again
            // while Comtrade still reports Viber Undelivered (runs every minute).
            campaignLead.SMSStatus = CampaignLeadSMSStatus.Pending;
            campaignLead.SMSStatusDescription = "SMS dispatch in progress.";
            await unitOfWork.SaveChangesAsync();

            // message personalization
            string message = stringTemplatingService.SubstituteLeadInfo(campaign.SmsMessage!, lead);

            // send SMS
            SendSmsRequestDto request = new()
            {
                Sender = campaign.SmsSender!,
                Message = message,
                Phone = PhoneNumberHelper.ForSmsGateway(lead.PhoneNumber)
            };

            SendSmsResponseDto? response;
            try
            {
                response = await smsService.Send(
                    request,
                    company.SmsUsername ?? string.Empty,
                    company.SmsPassword ?? string.Empty);
            }
            catch (Exception)
            {
                campaignLead.SMSStatus = CampaignLeadSMSStatus.SendingError;
                campaignLead.SMSStatusDescription = "SMS gateway request failed.";
                await unitOfWork.SaveChangesAsync();
                await Task.Delay(200);
                continue;
            }

            if (response is null)
            {
                // Leave Pending: gateway may have accepted the message; avoids repeated fallback sends on transient errors.
                campaignLead.SMSStatusDescription = "SMS gateway returned no parseable response.";
                await unitOfWork.SaveChangesAsync();
                await Task.Delay(200);
                continue;
            }

            campaignLead.SMSReferenceId = response.Reference;

            switch (response.Status)
            {
                case SmsStatus.Success:
                    campaignLead.SMSStatus = CampaignLeadSMSStatus.Pending;
                    campaignLead.SMSStatusDescription = null;
                    break;
                case SmsStatus.InvalidPhoneNumber:
                    campaignLead.SMSStatus = CampaignLeadSMSStatus.InvalidPhone;
                    campaignLead.SMSStatusDescription = response.Message;
                    break;
                default:
                    campaignLead.SMSStatus = CampaignLeadSMSStatus.SendingError;
                    campaignLead.SMSStatusDescription = string.IsNullOrWhiteSpace(response.Message)
                        ? $"SMS status {(int)response.Status}"
                        : response.Message;
                    break;
            }

            await unitOfWork.SaveChangesAsync();

            // pause for 200 ms before another API call
            await Task.Delay(200);
        }

        // await Task.Delay(10000);
        // await SmsDeliveryService.GetSmsDeliveryForCampaignId(
        //     campaign.Id,
        //     companyRepository,
        //     campaignRepository,
        //     campaignLeadRepository,
        //     httpClientFactory,
        //     unitOfWork);
    }

    public static async Task SendSmsToTestPhoneNumbers(
        Campaign campaign,
        TestMessage testMessage,
        ICompanyRepository companyRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork)
    {
        // get company (for SMS credentials)
        Company company = await companyRepository.GetByIdAsync(campaign.CompanyId)
            ?? throw new Exception($"Company [{campaign.CompanyId.Value}] - not found.");

        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
        SmsService smsService = new(httpClient);

        // send SMS
        SendSmsRequestDto request = new()
        {
            Sender = campaign.SmsSender!,
            Message = campaign.SmsMessage!,
            Phone = PhoneNumberHelper.ForSmsGateway(testMessage.PhoneNumber)
        };

        SendSmsResponseDto? response = await smsService.Send(
            request,
            company.SmsUsername ?? string.Empty,
            company.SmsPassword ?? string.Empty);

        // handle response
        if (response is not null)
        {
            switch (response.Status)
            {
                case SmsStatus.Success:
                {
                    testMessage.IsDelivered = true;
                    break;
                }
            }

            testMessage.SmsReferenceId = response.Reference;
        }
        
        await unitOfWork.SaveChangesAsync();
    }

    public static async Task SendApiSmsMessages(
        CompanyId companyId,
        List<ApiMessage> apiMessages,
        ICompanyRepository companyRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork)
    {
        // get company (for SMS credentials)
        Company company = await companyRepository.GetByIdAsync(companyId)
                          ?? throw new Exception($"Company [{companyId.Value}] - not found.");
        
        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
        SmsService smsService = new(httpClient);
        
        // send SMS to all leads one-by-one
        foreach (ApiMessage apiMessage in apiMessages)
        {
            // send SMS
            SendSmsRequestDto request = new()
            {
                Sender = apiMessage.SmsSender ?? apiMessage.Sender!,
                Message = apiMessage.SmsMessage ?? apiMessage.Message,
                Phone = PhoneNumberHelper.ForSmsGateway(apiMessage.PhoneNumber)
            };

            SendSmsResponseDto? response = await smsService.Send(
                request,
                company.SmsUsername ?? string.Empty,
                company.SmsPassword ?? string.Empty);

            // handle response
            if (response is not null)
            {
                // @TODO: handle more cases
                switch (response.Status)
                {
                    case SmsStatus.Success:
                        {
                            apiMessage.SMSStatus = CampaignLeadSMSStatus.Pending;
                            break;
                        }
                    case SmsStatus.InvalidPhoneNumber:
                        {
                            apiMessage.SMSStatus = CampaignLeadSMSStatus.InvalidPhone;
                            break;
                        }
                }

                apiMessage.SMSReferenceId = response.Reference;
                apiMessage.IsSent = true;
            }

            // pause for 200 ms before another API call
            await Task.Delay(200);
        }

        await unitOfWork.SaveChangesAsync();
    }
}