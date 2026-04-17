using OneClickEcho.Application.Common.Services.SmsService.Request;
using OneClickEcho.Application.Common.Services.SmsService.Response;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.CampaignAggregate;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Domain.TestMessageAggregate.Repositories;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Sms;

public class SmsDeliveryService
{
    public static async Task GetSmsDeliveryForCampaignId(
        CampaignId campaignId,
        ICompanyRepository companyRepository,
        ICampaignRepository campaignRepository,
        ICampaignLeadRepository campaignLeadRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork)
    {
        // Get campaign
        Campaign campaign = await campaignRepository.GetByIdAsync(campaignId)
                            ?? throw new Exception($"Campaign [{campaignId}] not found.");

        // get company (for SMS credentials)
        Company company = await companyRepository.GetByIdAsync(campaign.CompanyId)
                          ?? throw new Exception($"Company [{campaign.CompanyId.Value}] - not found.");

        if (!campaign.IsSms)
        {
            throw new Exception($"Unable to fetch delivery report for campaign [{campaignId}] since it is not an SMS campaign.");
        }

        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
        SmsService smsService = new(httpClient);
        string smsUser = company.SmsUsername ?? string.Empty;
        string smsPwd = company.SmsPassword ?? string.Empty;

        List<CampaignLead> campaignLeads = campaignLeadRepository
            .GetAllCampaignLeadsAsync(campaignId).Result;
        // .Where(x =>
        //     x.ViberStatus != CampaignLeadViberStatus.Seen &&
        //     x.ViberStatus != CampaignLeadViberStatus.Undelivered &&
        //     x.ViberStatus != CampaignLeadViberStatus.Expired)
        // .ToList();

        //List<LeadId> undeliveredCampaignLeads = [];

        foreach (CampaignLead campaignLead in campaignLeads)
        {
            if (string.IsNullOrWhiteSpace(campaignLead.SMSReferenceId))
            {
                continue;
            }

            SendSmsDeliveryRequestDto request = new()
            {
                Reference = campaignLead.SMSReferenceId,
            };

            SendSmsDeliveryResponseDto? response = await smsService
                .GetDelivery(request, smsUser, smsPwd)
                ?? throw new Exception($"Failed to get delivery response for campaign [{campaign.Id.Value}].");

            // Console.WriteLine($"SMS delivery response: {response}");

            campaignLead.SMSStatus = (CampaignLeadSMSStatus)response.StatusVal;

            //if ((SmsDeliveryStatus)response.StatusVal == SmsDeliveryStatus.Undelivered)
            //{
            //    undeliveredCampaignLeads.Add(campaignLead.LeadId);
            //}
        }

        await unitOfWork.SaveChangesAsync();
    }
    
    public static async Task GetSmsDeliveryForApiMessages(
        CompanyId companyId,
        List<ApiMessage> apiMessages,
        ICompanyRepository companyRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork)
    {
        // get company (for SMS credentials)
        Company company = await companyRepository.GetByIdAsync(companyId)
                          ?? throw new Exception($"Company [{companyId}] - not found.");

        HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
        SmsService smsService = new(httpClient);
        string smsUser = company.SmsUsername ?? string.Empty;
        string smsPwd = company.SmsPassword ?? string.Empty;

        foreach (ApiMessage apiMessage in apiMessages)
        {
            if (string.IsNullOrWhiteSpace(apiMessage.SMSReferenceId))
            {
                continue;
            }

            SendSmsDeliveryRequestDto request = new()
            {
                Reference = apiMessage.SMSReferenceId,
            };

            SendSmsDeliveryResponseDto? response = await smsService
                .GetDelivery(request, smsUser, smsPwd)
                ?? throw new Exception($"Failed to get delivery response for SMS API message [{apiMessage.SMSReferenceId}].");

            // Console.WriteLine($"SMS delivery response: {response}");

            apiMessage.SMSStatus = (CampaignLeadSMSStatus)response.StatusVal;
        }

        await unitOfWork.SaveChangesAsync();
    }
    
    public static async Task GetSmsTestDeliveryForLast48Hours(
        ICompanyRepository companyRepository,
        ITestMessageRepository testMessageRepository,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork)
    {
        List<TestMessage> testMessages = await testMessageRepository.GetSmsTestMessagesForLast48Hours();

        foreach (TestMessage testMessage in testMessages)
        {
            // get company (for SMS credentials)
            Company company = await companyRepository.GetByIdAsync(testMessage.CompanyId)
                              ?? throw new Exception($"Company [{testMessage.CompanyId.Value}] - not found.");
            
            HttpClient httpClient = httpClientFactory.CreateClient("SmsHttpClient");
            SmsService smsService = new(httpClient);
            string smsUser = company.SmsUsername ?? string.Empty;
            string smsPwd = company.SmsPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(testMessage.SmsReferenceId))
            {
                continue;
            }

            SendSmsDeliveryRequestDto request = new()
            {
                Reference = testMessage.SmsReferenceId,
            };

            SendSmsDeliveryResponseDto? response = await smsService
                .GetDelivery(request, smsUser, smsPwd)
                ?? throw new Exception($"Failed to get test delivery response for message [{testMessage.SmsReferenceId}].");

            // Console.WriteLine($"SMS delivery response: {response}");

            if ((CampaignLeadSMSStatus)response.StatusVal == CampaignLeadSMSStatus.Delivered)
            {
                testMessage.IsDelivered = true;
            }
        }
        
        await unitOfWork.SaveChangesAsync();
    }
}