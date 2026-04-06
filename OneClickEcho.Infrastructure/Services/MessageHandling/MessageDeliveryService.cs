using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.CompanyAggregate.Repositories;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.LeadAggregate.Repositories;
using OneClickEcho.Domain.TestMessageAggregate.Repositories;
using OneClickEcho.Infrastructure.Services.MessageHandling.Sms;
using OneClickEcho.Infrastructure.Services.MessageHandling.Viber;
using OneClickEcho.Infrastructure.Settings;

namespace OneClickEcho.Infrastructure.Services.MessageHandling;

public class MessageDeliveryService(ICampaignRepository campaignRepository,
    ILeadRepository leadRepository, ICampaignLeadRepository campaignLeadRepository,
    ICompanyRepository companyRepository, IApiMessageRepository apiMessageRepository,
    ITestMessageRepository testMessageRepository, IStringTemplatingService stringTemplatingService,
    IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, IOptions<ViberSettings> viberSettings)
    : IMessageDeliveryService
{
    private readonly ICampaignRepository _campaignRepository = campaignRepository;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;
    private readonly IApiMessageRepository _apiMessageRepository = apiMessageRepository;
    private readonly ITestMessageRepository _testMessageRepository = testMessageRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly IStringTemplatingService _stringTemplatingService = stringTemplatingService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOptions<ViberSettings> _viberSettings = viberSettings;

    public async Task GetViberDeliveryForLast49Hours()
    {
        await ViberDeliveryService.GetViberDeliveryForLast49Hours(
            _companyRepository,
            _campaignRepository,
            _campaignLeadRepository,
            _httpClientFactory,
            _viberSettings,
            _leadRepository,
            _stringTemplatingService,
            _unitOfWork);
    }

    public async Task GetViberAnswersForLast49Hours()
    {
        await ViberDeliveryService.GetViberAnswersForLast49Hours(
            _campaignRepository,
            _campaignLeadRepository,
            _httpClientFactory,
            _viberSettings,
            _unitOfWork);
    }

    public async Task GetApiMessagesDelivery()
    {
        List<ApiMessage> apiMessages = await _apiMessageRepository.GetUnsentApiMessages(DateTime.Now.AddHours(-1));
        
        if (!apiMessages.Any())
        {
            // Console.WriteLine($"{DateTime.Now} - No API messages pending delivery update.");
            return;
        }
        
        var messageGroups = apiMessages
            .GroupBy(m => new { m.CompanyId, m.MessageType })
            .Where(g => g.Any())
            .ToList();

        foreach (var messageGroup in messageGroups)
        {
            if (!messageGroup.Any())
            {
                // Console.WriteLine($"{DateTime.Now} - No API messages pending delivery update company ID [{messageGroup.Key.CompanyId}] and message type [{messageGroup.Key.MessageType}]");
                return;
            }

            if (messageGroup.Key.MessageType == ApiMessageType.Viber)
            {
                await GetViberDeliveryForApiMessages(messageGroup.Key.CompanyId, messageGroup.ToList());
            }
            else if (messageGroup.Key.MessageType == ApiMessageType.Sms)
            {
                await GetSmsDeliveryForApiMessages(messageGroup.Key.CompanyId, messageGroup.ToList());
            }
        }
    }

    public async Task GetViberDeliveryForApiMessages(CompanyId companyId, List<ApiMessage> messages)
    {
        if (!messages.Any())
        {
            return;
        }

        await ViberDeliveryService.GetViberDeliveryForApiMessages(
            companyId,
            messages,
            _companyRepository,
            _httpClientFactory,
            _viberSettings,
            _leadRepository,
            _unitOfWork);
    }

    public async Task GetSmsDeliveryForApiMessages(CompanyId companyId, List<ApiMessage> messages)
    {
        if (!messages.Any())
        {
            return;
        }

        await SmsDeliveryService.GetSmsDeliveryForApiMessages(
            companyId,
            messages,
            _companyRepository,
            _httpClientFactory,
            _unitOfWork);
    }

    public async Task GetViberTestDeliveryForLast48Hours()
    {
        await ViberDeliveryService.GetViberTestDeliveryForLast48Hours(
            _testMessageRepository,
            _httpClientFactory,
            _viberSettings,
            _unitOfWork);
    }

    public async Task GetSmsTestDeliveryForLast48Hours()
    {
        await SmsDeliveryService.GetSmsTestDeliveryForLast48Hours(
            _companyRepository,
            _testMessageRepository,
            _httpClientFactory,
            _unitOfWork);
    }

    public async Task GetSmsDeliveryForCampaignId(CampaignId campaign)
    {
        await SmsDeliveryService.GetSmsDeliveryForCampaignId(
            campaign,
            _companyRepository,
            _campaignRepository,
            _campaignLeadRepository,
            _httpClientFactory,
            _unitOfWork);
    }
}