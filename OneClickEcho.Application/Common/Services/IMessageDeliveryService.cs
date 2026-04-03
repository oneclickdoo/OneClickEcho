using OneClickEcho.Domain.ApiMessageAggregate;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Application.Common.Services;

public interface IMessageDeliveryService
{
    public Task GetViberDeliveryForLast49Hours();
    
    public Task GetViberAnswersForLast49Hours();
    
    public Task GetViberTestDeliveryForLast48Hours();
    
    public Task GetSmsTestDeliveryForLast48Hours();
    
    public Task GetSmsDeliveryForCampaignId(CampaignId campaign);

    public Task GetViberDeliveryForApiMessages(CompanyId companyId, List<Domain.ApiMessageAggregate.ApiMessage> messages);
    
    public Task GetSmsDeliveryForApiMessages(CompanyId companyId, List<Domain.ApiMessageAggregate.ApiMessage> messages);
}