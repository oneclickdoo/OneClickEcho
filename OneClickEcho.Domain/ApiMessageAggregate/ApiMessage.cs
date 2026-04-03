using OneClickEcho.Domain.ApiMessageAggregate.Enums;
using OneClickEcho.Domain.ApiMessageAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.ApiMessageAggregate;

public class ApiMessage : AggregateRoot<ApiMessageId>
{
    public ApiMessage(
        CompanyId companyId,
        string phoneNumber,
        string message,
        ApiMessageType apiMessageType,
        bool? hasSmsFallback,
        string? sender,
        string? viberMedia,
        string? viberButtonUrl,
        string? viberButtonUrlTitle,
        string? smsMessage,
        string? smsSender,
        int? viberValidity
    ) : base(ApiMessageId.CreateUnique())
    {
        CompanyId = companyId;
        PhoneNumber = phoneNumber;
        Message = message;
        MessageType = apiMessageType;
        HasSmsFallback = hasSmsFallback ?? false;
        Sender = sender;
        ViberMedia = viberMedia;
        ViberButtonUrl = viberButtonUrl;
        ViberButtonUrlTitle = viberButtonUrlTitle;
        SmsMessage = smsMessage;
        SmsSender = smsSender;
        ViberValidity = viberValidity;
    }
    
    public CompanyId CompanyId { get; set; } = default!;
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public ApiMessageType MessageType { get; set; }
    
    public string? Sender { get; set; }

    public bool IsSent { get; set; } = false;
    
    public bool HasSmsFallback { get; set; }

    public string? ViberMedia { get; set; }

    public string? ViberButtonUrl { get; set; }

    public string? ViberButtonUrlTitle { get; set; }
    
    public long ViberMessageId { get; set; }

    public CampaignLeadViberStatus ViberStatus { get; set; }

    public string? ViberStatusDescription { get; set; }

    public CampaignLeadSMSStatus SMSStatus { get; set; }

    public string? SMSStatusDescription { get; set; }

    public string? SMSReferenceId { get; set; }
    
    public string? SmsMessage { get; set; }
    public string? SmsSender { get; set; }
    public int? ViberValidity { get; set; }
    
    // Used for EFCore
    public ApiMessage() : base(ApiMessageId.CreateUnique()) { }
}