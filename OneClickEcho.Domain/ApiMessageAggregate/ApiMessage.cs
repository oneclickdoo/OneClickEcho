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
        int? viberValidity,
        string? viberVideoThumbnail = null,
        int? viberFileSize = null,
        int? viberVideoDuration = null,
        string? viberImageFormat = null,
        string? viberImageRatio = null,
        string? viberSurveyOptionsJson = null,
        short? viberSurveyUiType = null,
        string? viberCarouselJson = null,
        Guid? viberTemplateId = null,
        string? viberTemplateParamsJson = null
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
        ViberVideoThumbnail = viberVideoThumbnail;
        ViberFileSize = viberFileSize;
        ViberVideoDuration = viberVideoDuration;
        ViberImageFormat = viberImageFormat;
        ViberImageRatio = viberImageRatio;
        ViberSurveyOptionsJson = viberSurveyOptionsJson;
        ViberSurveyUiType = viberSurveyUiType;
        ViberCarouselJson = viberCarouselJson;
        ViberTemplateId = viberTemplateId;
        ViberTemplateParamsJson = viberTemplateParamsJson;
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

    /// <summary>Stored file name under public uploads, or absolute HTTPS URL. Viber/Comtrade must fetch thumbnail over the public internet.</summary>
    public string? ViberVideoThumbnail { get; set; }

    public int? ViberFileSize { get; set; }

    public int? ViberVideoDuration { get; set; }

    /// <summary>Explicit Comtrade ImageFormat: gif or img. Gif is also inferred from the media extension.</summary>
    public string? ViberImageFormat { get; set; }

    public string? ViberImageRatio { get; set; }

    public string? ViberSurveyOptionsJson { get; set; }

    public short? ViberSurveyUiType { get; set; }

    public string? ViberCarouselJson { get; set; }

    public Guid? ViberTemplateId { get; set; }

    public string? ViberTemplateParamsJson { get; set; }

    // Used for EFCore
    public ApiMessage() : base(ApiMessageId.CreateUnique()) { }
}