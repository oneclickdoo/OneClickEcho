using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Entities;
using OneClickEcho.Domain.CampaignLeadAggregate.Enums;
using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.LeadAggregate.ValueObjects;

namespace OneClickEcho.Domain.CampaignLeadAggregate;

public sealed class CampaignLead : AggregateRoot<CampaignLeadId>
{
    public CampaignLead(
        CampaignLeadId id,
        LeadId leadId,
        CampaignId campaignId,
        bool isBlacklisted,
        long viberMessageId,
        CampaignLeadViberStatus viberStatus,
        string? viberStatusDescription,
        CampaignLeadSMSStatus smsStatus,
        string? smsStatusDescription
    ) : base(id)
    {
        LeadId = leadId;
        CampaignId = campaignId;
        IsBlacklisted = isBlacklisted;
        ViberMessageId = viberMessageId;
        ViberStatus = viberStatus;
        ViberStatusDescription = viberStatusDescription;
        SMSStatus = smsStatus;
        SMSStatusDescription = smsStatusDescription;
    }

    public CampaignLead(
        LeadId leadId,
        CampaignId campaignId
    ) : base(CampaignLeadId.CreateUnique())
    {
        LeadId = leadId;
        CampaignId = campaignId;
        IsBlacklisted = false;
        ViberStatus = CampaignLeadViberStatus.None;
        SMSStatus = CampaignLeadSMSStatus.None;
    }

    public LeadId LeadId { get; set; } = default!;

    public CampaignId CampaignId { get; set; } = default!;

    public bool IsBlacklisted { get; set; }

    public long ViberMessageId { get; set; }

    public CampaignLeadViberStatus ViberStatus { get; set; }

    public string? ViberStatusDescription { get; set; }

    public CampaignLeadSMSStatus SMSStatus { get; set; }

    public string? SMSStatusDescription { get; set; }

    public string? SMSReferenceId { get; set; }

    public ICollection<ReceivedMessage> ReceivedMessages { get; } = [];

    // Used for EFCore
    public CampaignLead() : base(CampaignLeadId.CreateUnique()) { }
}