using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.CountCampaignLeadsByStatus;

public record CountCampaignLeadsByStatusQuery(Guid CampaignId) : IQuery<CountCampaignLeadsByStatusResponse>;