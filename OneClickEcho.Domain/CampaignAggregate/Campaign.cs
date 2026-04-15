using OneClickEcho.Domain.CampaignAggregate.Entities;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.CampaignAggregate;

public sealed class Campaign : AggregateRoot<CampaignId>
{
    public Campaign(
        CampaignId id,
        CampaignStatus status,
        CompanyId companyId,
        string name,
        bool isViber,
        bool fallbackToSMS,
        bool isViberReceivable,
        string? viberSender,
        string? viberMessage,
        string? viberMedia,
        string? viberButtonUrl,
        string? viberButtonUrlTitle,
        int? viberFileSize,
        string? viberVideoThumbnail,
        int? viberVideoDuration,
        bool isSms,
        string? smsSender,
        string? smsMessage,
        string? testPhoneNumber,
        CampaignSendingType sendingType,
        DateTime? sendingDatetime
    ) : base(id)
    {
        Status = status;
        CompanyId = companyId;
        Name = name;
        IsViber = isViber;
        FallbackToSMS = fallbackToSMS;
        IsViberReceivable = isViberReceivable;
        ViberSender = viberSender;
        ViberMessage = viberMessage;
        ViberMedia = viberMedia;
        ViberButtonUrl = viberButtonUrl;
        ViberButtonUrlTitle = viberButtonUrlTitle;
        ViberFileSize = viberFileSize;
        ViberVideoThumbnail = viberVideoThumbnail;
        ViberVideoDuration = viberVideoDuration;
        IsSms = isSms;
        SmsSender = smsSender;
        SmsMessage = smsMessage;
        TestPhoneNumber = testPhoneNumber;
        SendingType = sendingType;
        SendingDatetime = sendingDatetime ?? DateTime.UtcNow;
        ViberValidity = 86400;
    }

    public Campaign(
        CampaignStatus status,
        CompanyId companyId,
        CampaignSendingType sendingType,
        string name = "",
        string? viberSender = null,
        string? viberMessage = null,
        string? viberMedia = null,
        string? viberButtonUrl = null,
        string? viberButtonUrlTitle = null,
        int? viberFileSize = null,
        string? viberVideoThumbnail = null,
        int? viberVideoDuration = null,
        string? smsSender = null,
        string? smsMessage = null,
        string? testPhoneNumber = null,
        DateTime? sendingDatetime = null,
        bool isSms = false,
        bool isViber = false,
        bool fallbackToSMS = false,
        bool isViberReceivable = false
    ) : base(CampaignId.CreateUnique())
    {
        Status = status;
        CompanyId = companyId;
        Name = name;
        IsViber = isViber;
        FallbackToSMS = fallbackToSMS;
        IsViberReceivable = isViberReceivable;
        ViberSender = viberSender;
        ViberMessage = viberMessage;
        ViberMedia = viberMedia;
        ViberButtonUrl = viberButtonUrl;
        ViberButtonUrlTitle = viberButtonUrlTitle;
        ViberFileSize = viberFileSize;
        ViberVideoThumbnail = viberVideoThumbnail;
        ViberVideoDuration = viberVideoDuration;
        IsSms = isSms;
        SmsSender = smsSender;
        SmsMessage = smsMessage;
        TestPhoneNumber = testPhoneNumber;
        SendingType = sendingType;
        SendingDatetime = sendingDatetime ?? DateTime.UtcNow;
        ViberValidity = 86400;
    }

    public CampaignStatus Status { get; set; }

    public CompanyId CompanyId { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public bool IsViber { get; set; }

    public bool FallbackToSMS { get; set; }

    public bool IsViberReceivable { get; set; }

    public string? ViberSender { get; set; }

    public string? ViberMessage { get; set; }

    public string? ViberMedia { get; set; }

    public string? ViberButtonUrl { get; set; }

    public string? ViberButtonUrlTitle { get; set; }

    public int? ViberFileSize { get; set; }

    public string? ViberVideoThumbnail { get; set; }

    public int? ViberVideoDuration { get; set; }

    /// <summary>Explicit Viber layout; when <see cref="CampaignViberContentKind.Text"/> and <see cref="ViberMedia"/> is set, sending infers image/video/file from media.</summary>
    public CampaignViberContentKind ViberContentKind { get; set; }

    /// <summary>JSON array of 2–5 strings (Comtrade survey options), used when <see cref="ViberContentKind"/> is <see cref="CampaignViberContentKind.Survey"/>.</summary>
    public string? ViberSurveyOptionsJson { get; set; }
    
    public int? ViberValidity { get; set; }

    public bool IsSms { get; set; }

    public string? SmsSender { get; set; }

    public string? SmsMessage { get; set; }
    
    public string? TestPhoneNumber { get; set; }

    public CampaignSendingType SendingType { get; set; }

    public DateTime SendingDatetime { get; set; } = DateTime.UtcNow;

    public ICollection<CampaignLeadCollection> LeadCollections { get; set; } = [];

    // Used for EFCore
    public Campaign(string name) : base(CampaignId.CreateUnique())
    {
        Name = name;
    }
}