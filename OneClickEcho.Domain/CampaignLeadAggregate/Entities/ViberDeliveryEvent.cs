using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignLeadAggregate.Entities;

public sealed class ViberDeliveryEvent : Entity<ViberDeliveryEventId>
{
    public ViberDeliveryEvent(
        CampaignLeadId campaignLeadId,
        long viberMessageId,
        short status,
        int subStatus,
        int clickCount) : base(ViberDeliveryEventId.CreateUnique())
    {
        CampaignLeadId = campaignLeadId;
        ViberMessageId = viberMessageId;
        Status = status;
        SubStatus = subStatus;
        ClickCount = clickCount;
    }

    public CampaignLeadId CampaignLeadId { get; set; } = default!;
    public long ViberMessageId { get; set; }
    public short Status { get; set; }
    public int SubStatus { get; set; }
    public int ClickCount { get; set; }

    // Used for EFCore
    public ViberDeliveryEvent() : base(ViberDeliveryEventId.CreateUnique())
    {
    }
}
