using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.CampaignLeadAggregate.Repositories;
using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeads
{
    public class GetCampaignLeadsHandler(ICampaignRepository campaignRepository, ICampaignLeadRepository campaignLeadRepository)
        : IQueryHandler<GetCampaignLeadsQuery, GetCampaignLeadsResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ICampaignLeadRepository _campaignLeadRepository = campaignLeadRepository;

        public async Task<Result<GetCampaignLeadsResponse>> Handle(GetCampaignLeadsQuery query, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(query.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<GetCampaignLeadsResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{query.CampaignId}\" does not exist."
                    ));
            }

            // get campaign leads
            Domain.Common.Queries.IPagedList<Domain.LeadAggregate.Lead> campaignLeads = await _campaignLeadRepository
                .GetPagedLeadsByCampaignIdAsync(campaign.Id, query, cancellationToken);

            return new GetCampaignLeadsResponse(campaignLeads);
        }
    }
}
