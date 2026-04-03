using FluentValidation;
using OneClickEcho.Application.Common.Abstractions;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignLeadCollections;

public class GetCampaignLeadCollectionsValidator : BasePagedValidator<GetCampaignLeadCollectionsQuery>
{
    public GetCampaignLeadCollectionsValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignID is required.");
    }
}