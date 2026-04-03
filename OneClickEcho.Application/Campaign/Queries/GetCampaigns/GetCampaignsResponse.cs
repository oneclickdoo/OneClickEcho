using OneClickEcho.Application.Common.Abstractions;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.Common.Queries;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaigns;

public class GetCampaignsResponse(IPagedList<Domain.CampaignAggregate.Campaign> items)
    : PagedListDto<Domain.CampaignAggregate.Campaign, GetCampaignDto>(items)
{
    public override List<GetCampaignDto> ConvertTToTDto(List<Domain.CampaignAggregate.Campaign> items)
    {
        List<GetCampaignDto> result = [];

        foreach (Domain.CampaignAggregate.Campaign campaign in items)
        {
            result.Add(new GetCampaignDto(
                CampaignId: campaign.Id.Value,
                CompanyId: campaign.CompanyId.Value,
                Status: campaign.Status,
                Name: campaign.Name,
                SendingDatetime: campaign.SendingDatetime,
                CreatedAt: campaign.CreatedAt
            ));
        }

        return result;
    }
}

public record GetCampaignDto(
    Guid CampaignId,
    Guid CompanyId,
    CampaignStatus Status,
    string Name,
    DateTime? SendingDatetime,
    DateTime CreatedAt
);