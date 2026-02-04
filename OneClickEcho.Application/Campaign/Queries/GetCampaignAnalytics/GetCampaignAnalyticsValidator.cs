using FluentValidation;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignAnalytics
{
    public sealed class GetCampaignAnalyticsValidator : AbstractValidator<GetCampaignAnalyticsQuery>
    {
        public GetCampaignAnalyticsValidator()
        {
            RuleFor(query => query.CampaignId)
                .NotEmpty()
                .WithMessage("CampaignId must not be empty.");
        }
    }
}
