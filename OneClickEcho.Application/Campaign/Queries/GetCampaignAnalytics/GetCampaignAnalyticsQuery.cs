using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics
{
    public record GetCampaignAnalyticsQuery(Guid CampaignId) : IQuery<GetCampaignAnalyticsResponse>;
}
