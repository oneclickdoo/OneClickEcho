using FluentValidation;

namespace OneClickEcho.Application.Campaign.Queries.CountCampaignLeadsByStatus;

public class CountCampaignLeadsByStatusValidator : AbstractValidator<CountCampaignLeadsByStatusQuery>
{
    public CountCampaignLeadsByStatusValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignID is required.");
    }
}