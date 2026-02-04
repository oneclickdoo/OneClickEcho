using FluentValidation;

namespace OneClickEcho.Application.LeadCollection.Queries.SearchLeadCollections;

public class SearchLeadCollectionsValidator : AbstractValidator<SearchLeadCollectionsQuery>
{
    public SearchLeadCollectionsValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("CampaignId must be a valid GUID.");
    }
}