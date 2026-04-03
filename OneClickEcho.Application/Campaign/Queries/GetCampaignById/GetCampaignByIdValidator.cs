using FluentValidation;

namespace OneClickEcho.Application.Campaign.Queries.GetCampaignById;

public sealed class GetCampaignByIdValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdValidator()
    {
        RuleFor(query => query.CampaignId)
            .NotEmpty()
            .WithMessage("CampaignId must not be empty.");
    }
}