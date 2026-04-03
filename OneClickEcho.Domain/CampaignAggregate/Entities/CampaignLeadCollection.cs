using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.LeadCollectionAggregate.ValueObjects;

namespace OneClickEcho.Domain.CampaignAggregate.Entities;

public sealed class CampaignLeadCollection : Entity<CampaignLeadCollectionId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CampaignLeadCollection(CampaignLeadCollectionId campaignLeadCollectionId, LeadCollectionId leadCollectionId) : base(campaignLeadCollectionId)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        LeadCollectionId = leadCollectionId;
    }

    public CampaignLeadCollection(
        CampaignId campaignId,
        LeadCollectionId leadCollectionId
        ) : base(CampaignLeadCollectionId.CreateUnique())
    {
        CampaignId = campaignId;
        LeadCollectionId = leadCollectionId;
    }

    public CampaignId CampaignId { get; set; }

    public LeadCollectionId LeadCollectionId { get; set; } = default!;

    // Used for EFCore
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CampaignLeadCollection() : base(CampaignLeadCollectionId.CreateUnique())
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }
}