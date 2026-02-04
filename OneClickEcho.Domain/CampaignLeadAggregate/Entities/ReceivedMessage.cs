using OneClickEcho.Domain.CampaignLeadAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CampaignLeadAggregate.Entities;

public sealed class ReceivedMessage : Entity<ReceivedMessageId>
{
    public ReceivedMessage(
        string messageContent,
        CampaignLeadId campaignLeadId) : base(ReceivedMessageId.CreateUnique())
    {
        MessageContent = messageContent;
        CampaignLeadId = campaignLeadId;
    }

    public CampaignLeadId CampaignLeadId { get; set; } = default!;

    public string MessageContent { get; set; } = string.Empty;

    // Used for EFCore
    public ReceivedMessage() : base(ReceivedMessageId.CreateUnique())
    {
    }
}