using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignById;

public record GetCampaignByIdQuery(Guid CampaignId) : IQuery<GetCampaignByIdResponse>;