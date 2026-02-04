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

        // create HTTP client
        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");

        // add authorization headers
        httpClient.DefaultRequestHeaders.Add("username", company.SmsUsername);
        httpClient.DefaultRequestHeaders.Add("pwd", company.SmsPassword);

        SmsService smsService = new(httpClient);

        // send SMS to all leads one-by-one
        foreach (Lead lead in leads)
        {
            // message personalization
            string message = stringTemplatingService.SubstituteLeadInfo(campaign.SmsMessage!, lead);

            // send SMS
            SendSmsRequestDto request = new()
            {
                Sender = campaign.SmsSender!,
                Message = message,
                Phone = lead.PhoneNumber
            };

            SendSmsResponseDto? response = await smsService.Send(request);

            // handle response
            if (response is not null)
            {
                // get campaign lead
                CampaignLead? campaignLead = await campaignLeadRepository
                    .GetByCampaignAndLeadId(campaign.Id, lead.Id)
                    ?? throw new Exception($"CampaignLead for Lead [{lead.Id}] is not found.");

                // @TODO: handle more cases
                switch (response.Status)
                {
                    case SmsStatus.Success:
                        {
                            campaignLead.SMSStatus = CampaignLeadSMSStatus.Delivered;
                            break;
                        }
                    case SmsStatus.InvalidPhoneNumber:
                        {
                            campaignLead.SMSStatus = CampaignLeadSMSStatus.InvalidPhone;
                            break;
                        }
                }

                campaignLead.SMSReferenceId = response.Reference;
            }

            // pause for 200 ms before another API call
            await Task.Delay(200);
        }

        await unitOfWork.SaveChangesAsync();

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

        // create HTTP client
        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");

        // add authorization headers
        httpClient.DefaultRequestHeaders.Add("username", company.SmsUsername);
        httpClient.DefaultRequestHeaders.Add("pwd", company.SmsPassword);

        SmsService smsService = new(httpClient);

        // send SMS
        SendSmsRequestDto request = new()
        {
            Sender = campaign.SmsSender!,
            Message = campaign.SmsMessage!,
            Phone = testMessage.PhoneNumber
        };

        SendSmsResponseDto? response = await smsService.Send(request);

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
        
        // create HTTP client
        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");

        // add authorization headers
        httpClient.DefaultRequestHeaders.Add("username", company.SmsUsername);
        httpClient.DefaultRequestHeaders.Add("pwd", company.SmsPassword);

        SmsService smsService = new(httpClient);
        
        // send SMS to all leads one-by-one
        foreach (ApiMessage apiMessage in apiMessages)
        {
            // send SMS
            SendSmsRequestDto request = new()
            {
                Sender = apiMessage.SmsSender ?? apiMessage.Sender!,
                Message = apiMessage.SmsMessage ?? apiMessage.Message,
                Phone = apiMessage.PhoneNumber
            };

            SendSmsResponseDto? response = await smsService.Send(request);

            // handle response
            if (response is not null)
            {
                // @TODO: handle more cases
                switch (response.Status)
                {
                    case SmsStatus.Success:
                        {
                            apiMessage.SMSStatus = CampaignLeadSMSStatus.Delivered;
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